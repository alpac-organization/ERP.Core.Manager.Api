using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Handlers
{
    public class CreatePermitApplicationHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager) : AlpacBaseHandler<CreatePermitApplicationCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(CreatePermitApplicationCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if(!access.IsSuccess) 
            {
                return access.ErrorResponse; 
            }

            #region Validaciones

            var collaborator = await _unitOfWork.Collaborators.Entities
                .FirstOrDefaultAsync(c => c.IdentificationNumber == request.IdentificationNumber && c.CompanyId == request.CompanyId, cancellationToken);

            if (collaborator is null)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    $"No se encontró un colaborador con el número de identificación {request.IdentificationNumber} en esta empresa.", 
                    "ERP:003"
                );
            }

            var hasPending = await _unitOfWork.PermitApplications.Entities
                .AnyAsync(per => per.CollaboratorId == collaborator.Id && per.Status == PermitApplicationStatus.Pending, cancellationToken);

            if (hasPending)
            {
                return _errorManager.ThrowBadRequest<bool>("Ya se encuentra un solicitud pendiente, cancelar la solicitud o esperar aprobación", "ERP:02");
            }

            #endregion Validaciones

            var permitApplication  = new Domain.Entities.Payroll.PermitApplication();


            if (access.Role!.RoleType == RoleType.Administrator || access.Role!.RoleType == RoleType.Operator || access.Role!.RoleType == RoleType.Manager)
            {
                permitApplication.StartDate = null;
                permitApplication.EndDate   = null;
                permitApplication.EndTime   = null;
                permitApplication.StartTime = null;
                permitApplication.Status    = PermitApplicationStatus.Pending;
                permitApplication.CollaboratorCode = collaborator.CollaboratorCode;
                permitApplication.CollaboratorId = collaborator.Id;

                var fullNames = new[] 
                { 
                    collaborator.FirstName, 
                    collaborator.SecondName, 
                    collaborator.FirstLastname, 
                    collaborator.SecondLastname 
                };

                permitApplication.RequestedBy = string.Join(" ", fullNames.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n?.Trim()));

                switch (request.PermitApplicationType)
                {
                    case PermitApplicationType.DonatedVacations :
                    {
                        var collaboratorToReceive = await _unitOfWork.Collaborators.Entities
                            .Where(col => col.IdentificationNumber == request.PermitApplicationDonatedVacations!.IdentificationCollaboratorToReceive)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (collaboratorToReceive is null)
                        {
                            return _errorManager.ThrowBadRequest<bool>(
                                $"El colaborador seleccionado para la donación de vacaciones no existe en el sistema!",
                                "ERP:006"
                            );   
                        }

                        var personalInformation = await _unitOfWork.Collaborators.Entities
                            .Where(col => col.IdentificationNumber == request.IdentificationNumber)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (personalInformation is null)
                        {
                            return _errorManager.ThrowBadRequest<bool>(
                                $"No se pudo encontrar la información de colaborador de este usuario",
                                "ERP:006"
                            );  
                        }

                        var personalVacationControl = await _unitOfWork.Vacations.Entities
                            .Where(col => col.CollaboratorId == personalInformation.Id)
                            .FirstOrDefaultAsync(cancellationToken);


                        if (personalVacationControl is null)
                        {
                            return _errorManager.ThrowBadRequest<bool>(
                                $"No se encontro un control de vacaciones",
                                "ERP:006"
                            );  
                        }

                        if (request.PermitApplicationDonatedVacations?.AmountDays is null)
                        {
                            return _errorManager.ThrowBadRequest<bool>(
                                $"Debe especificar la cantidad de dias a donar",
                                "ERP:006"
                            );
                        }

                        var vacationCotrolOfOtherCollaborator = await _unitOfWork.Vacations.Entities
                            .Where(col => col.Id == collaboratorToReceive.Id)
                            .FirstOrDefaultAsync(cancellationToken);


                        if (personalVacationControl.AvailableVacations < request.PermitApplicationDonatedVacations.AmountDays)
                        {
                            return _errorManager.ThrowBadRequest<bool>(
                                $"No cuenta con la cantidad de dias suficientes para donar la cantidad solicitada",
                                "ERP:006"
                            ); 
                        }
                        else
                        {

                            permitApplication.Type = PermitApplicationType.DonatedVacations;
                            permitApplication.AmountDays = request.PermitApplicationDonatedVacations.AmountDays;
                            permitApplication.IdentificationCollaboratorToReceive = request.PermitApplicationDonatedVacations.IdentificationCollaboratorToReceive;

                            //Registramos y guardamos cambios
                            await _unitOfWork.PermitApplications.CreateVacationRequest(permitApplication);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                        }

                        return true;
                    }
                    default : {
                        return _errorManager.ThrowBadRequest<bool>("Este tipo de solicitud no encuentra disponible", "001");
                    }
                }
            }

            return true;
        }

    //     private static bool IsValidDates(DateTime startDate, DateTime endDate)
    //     {
    //         return endDate >= startDate;
    //     }

    //     private static decimal CalculateBusinessDays(DateTime start, DateTime end)
    //     {
    //         int totalCalendarDays = (end.Date - start.Date).Days + 1;

    //         if (totalCalendarDays >= 7)
    //         {
    //             return totalCalendarDays;
    //         }

    //         decimal totalDays = 0;

    //         DateTime current = start.Date;

    //         while (current <= end.Date)
    //         {
    //             if (current.DayOfWeek == DayOfWeek.Saturday)
    //             {
    //                 totalDays += 0.5m;
    //             }
    //             else if (current.DayOfWeek != DayOfWeek.Sunday)
    //             {
    //                 totalDays += 1.0m;
    //             }

    //             current = current.AddDays(1);
    //         }

    //         return totalDays;
    //     }

    //     private async Task<bool> CheckOverlappingDates(Guid collaboratorId, DateTime start, DateTime end, CancellationToken ct)
    //     {
    //         return await _unitOfWork.PermitApplications.Entities
    //             .AnyAsync(vr => 
    //                 vr.CollaboratorId == collaboratorId &&
    //                 vr.Status != PermitApplicationStatus.Rejected && 
    //                 vr.Status != PermitApplicationStatus.Cancelled &&
    //                 start <= vr.EndDate && 
    //                 end >= vr.StartDate, ct);
    //     }
    }
}   