using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands;
using ERP.Core.Database.Domain.Enums;

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
                switch (request.PermitApplicationType)
                {
                    case PermitApplicationType.Vacation : 
                    {
                        var overlappingRequests = await CheckOverlappingDates(
                            collaborator.Id, request.PermitApplicationVacation!.StartDate, request.PermitApplicationVacation.EndDate, 
                            cancellationToken
                        );

                        if (overlappingRequests)
                        {
                            return _errorManager.ThrowBadRequest<bool>(
                                $"El colaborador ya tiene una solicitud fechas proporcionadas.",
                                "ERP:006"
                            );
                        } 

                        var vacationControl = await _unitOfWork.Vacations.Entities
                            .Where(col => col.CollaboratorId == collaborator.Id)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (vacationControl is null)
                        {
                            return _errorManager.ThrowBadRequest<bool>(
                                $"Este colaborador no cuenta con un control de vacaciones",
                                "ERP:006"
                            );
                        }

                        var isValid = IsValidDates(request.PermitApplicationVacation!.StartDate, request.PermitApplicationVacation.EndDate);

                        if (isValid is false)
                        {
                            return _errorManager.ThrowBadRequest<bool>(
                                $"La fecha de fin no puede ser menor a la fecha de inicio",
                                "ERP:006"
                            );
                        }

                        int requestedDays = (request.PermitApplicationVacation.EndDate.Date - request.PermitApplicationVacation.StartDate.Date).Days + 1;

                        if (vacationControl.AvailableVacations < requestedDays)
                        {
                            return _errorManager.ThrowBadRequest<bool>(
                                $"La cantidad de dias es mayor a la cantidad de dias disponibles",
                                "ERP:006"
                            );
                        }

                        var fullNames = new[] 
                        { 
                            collaborator.FirstName, 
                            collaborator.SecondName, 
                            collaborator.FirstLastname, 
                            collaborator.SecondLastname 
                        };

                        permitApplication.RequestedBy = string.Join(" ", fullNames.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n?.Trim()));
                        permitApplication.StartDate = request.PermitApplicationVacation.StartDate;
                        permitApplication.EndDate = request.PermitApplicationVacation.EndDate;
                        permitApplication.AmountDays = requestedDays;
                        permitApplication.CollaboratorId = collaborator.Id;
                        permitApplication.Status = PermitApplicationStatus.Pending;
                        permitApplication.Type = PermitApplicationType.Vacation;
                        permitApplication.CollaboratorCode = collaborator.CollaboratorCode;

                        await _unitOfWork.PermitApplications.CreateVacationRequest(permitApplication);

                        // Solicitud Registrada con exito.
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        break;
                    }

                    case PermitApplicationType.DonatedVacations :
                    {




                        break;   
                    }
                    default : {
                        return _errorManager.ThrowBadRequest<bool>("Este tipo de solicitud no encuentra disponible", "001");
                    }
                }
            }

            return true;
        }

        private static bool IsValidDates(DateTime startDate, DateTime endDate)
        {
            return endDate >= startDate;
        }

        private async Task<bool> CheckOverlappingDates(Guid collaboratorId, DateTime start, DateTime end, CancellationToken ct)
        {
            return await _unitOfWork.PermitApplications.Entities
                .AnyAsync(vr => 
                    vr.CollaboratorId == collaboratorId &&
                    vr.Status != PermitApplicationStatus.Rejected && 
                    vr.Status != PermitApplicationStatus.Cancelled &&
                    start <= vr.EndDate && 
                    end >= vr.StartDate, ct);
        }
    }
}   