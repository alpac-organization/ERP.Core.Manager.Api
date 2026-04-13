using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands;
using System.Text.Json;

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

            var permitApplication = new Domain.Entities.Payroll.PermitApplication
            {
                StartDate = null,
                EndDate = null,
                EndTime = null,
                StartTime = null,
                Status = PermitApplicationStatus.Pending,
                CollaboratorCode = collaborator.CollaboratorCode,
                CollaboratorId = collaborator.Id,
                Description = request.Description
            };

            var AdditionalData = new AdditionalDataPermitApplication();

            var fullNames = new[] 
            { 
                collaborator.FirstName, 
                collaborator.SecondName, 
                collaborator.FirstLastname, 
                collaborator.SecondLastname 
            };

            permitApplication.RequestedBy = string.Join(" ", fullNames.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n?.Trim()));

            var startTimeLimit = new TimeOnly(8, 0);
            var endTimeLimit   = new TimeOnly(14, 30);

            switch (request.PermitApplicationType)
            {
                case PermitApplicationType.MedicalAppointment:
                {
                    var medicalReq = request.PermitApplicationMedicalAppointment!;

                    if (medicalReq.StartTime < startTimeLimit || medicalReq.EndTime > endTimeLimit)
                    {
                        return _errorManager.ThrowBadRequest<bool>(
                            "El horario solicitado está fuera del rango permitido (08:00 AM - 02:30 PM).", 
                            "ERP:TIME_OUT_OF_RANGE"
                        );
                    }

                    MapperCaseDefaultValues(permitApplication, access.Role!.RoleType);
                    permitApplication.Type = PermitApplicationType.MedicalAppointment;
                    permitApplication.StartDate = request.PermitApplicationMedicalAppointment?.StartDate;
                    permitApplication.StartTime = request.PermitApplicationMedicalAppointment?.StartTime;
                    permitApplication.EndTime = request.PermitApplicationMedicalAppointment?.EndTime;
                    
                    //Calcular las horas que solicito para evitar, falsos positivos
                    var duration = medicalReq.EndTime!.Value - medicalReq.StartTime!.Value;
                    decimal totalHours = (decimal)duration.TotalHours;

                    if (totalHours >= 5)
                    {
                        AdditionalData.MedicalAppointmentData.IsFullDay = true;   
                    }
                    else
                    {
                        AdditionalData.MedicalAppointmentData.IsFullDay = request.PermitApplicationMedicalAppointment?.IsFullDay ?? false;
                        permitApplication.AmountDays = 0;
                    }

                    permitApplication.AmountDays = 0.5m;

                    break;
                }

                case PermitApplicationType.DonatedVacations:
                {
                    //Validamos la cedula de la persona que viene a recibir el gozo de vacaciones donadas.
                    if (request.PermitApplicationDonatedVacations?.IdentificationCollaboratorToReceive is null)
                    {
                        return _errorManager.ThrowBadRequest<bool>("La identificación del colaborador que recibiras las vacaciones es requerido!", "ERP:02"); 
                    }

                    var collaboratoToReceive = await _unitOfWork.Collaborators.Entities
                        .Where(col => col.IdentificationNumber == request.PermitApplicationDonatedVacations!.IdentificationCollaboratorToReceive)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (collaboratoToReceive is null)
                    {
                        return _errorManager.ThrowBadRequest<bool>("El colaborador beneficiado por las vacaciones donadas no se existe!", "ERP:03");       
                    }

                    var vacationControl = await _unitOfWork.Vacations.Entities
                        .Where(vac => vac.CollaboratorId == collaborator.Id)
                        .FirstOrDefaultAsync(cancellationToken);


                    if (vacationControl is null)
                    {
                        return _errorManager.ThrowBadRequest<bool>("No se encontro su control de vacaciones no puede generar esta solicitud", "ERP:04");       
                    }

                    if (vacationControl.AvailableVacations < request.PermitApplicationDonatedVacations.AmountDays)
                    {
                         return _errorManager.ThrowBadRequest<bool>("No se cuentas con sufientes dias de vacaciones para donar", "ERP:05");       
                    }

                    //Mapeamos la data para crear la solicitud de permiso
                    MapperCaseDefaultValues(permitApplication, access.Role!.RoleType);
                    permitApplication.Type = PermitApplicationType.DonatedVacations;
                    permitApplication.AmountDays = request.PermitApplicationDonatedVacations?.AmountDays ?? 0;
                    permitApplication.IdentificationCollaboratorToReceive = request.PermitApplicationDonatedVacations?.IdentificationCollaboratorToReceive ?? string.Empty;

                    break;   
                }


                default:    
                {
                    return _errorManager.ThrowBadRequest<bool>("Este tipo de solicitud no se encuentra disponible de momento", "ERP:ErrorRequest"); 
                }
            }

            //Registrar Solicitud
            permitApplication.AdditionalData = JsonSerializer.Serialize(AdditionalData);
            await _unitOfWork.PermitApplications.CreateVacationRequest(permitApplication);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        public static async void MapperCaseDefaultValues(Domain.Entities.Payroll.PermitApplication entity, RoleType role)
        {
            if (role == RoleType.Administrator || role == RoleType.Manager)
            {
                entity.FirtsStepApproved = true;    
                entity.ManagerFullname = "Control Administración";
            }
            else if (role == RoleType.Operator)
            {
                entity.FirtsStepApproved = null;
            }
        }
    }
}   