using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Utils;

//Alias
using PermitApplicationEntity = ERP.Core.Database.Domain.Entities.Payrolls.PermitApplication;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Handlers
{
    public class CreatePermitApplicationHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<CreatePermitApplicationHandler> _logger) : AlpacBaseHandler<CreatePermitApplicationCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(CreatePermitApplicationCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if(!access.IsSuccess) 
            {
                return access.ErrorResponse; 
            }

            #region Validaciones

            var user = await _unitOfWork.Users.Entities
                .Where(user => user.Id == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    $"Ocurrio un error, este usuario no existe", 
                    "ERP:003"
                );
            }

            if (user.IdentificationNumber == request.IdentificationNumber && (request.Channel != Channels.PersonalPanel))
            {
                return _errorManager.ThrowBadRequest<bool>(
                    $"Ocurrio un error no puedes registrar esta solicitud desde este panel, realiza tu solicitud personal desde el panel de solicitudes en gestion de colaboradores", 
                    "ERP:003"
                );
            }

            var payrollActive = await _unitOfWork.Payrolls.Entities
                .Where(payroll => payroll.Id == request.PayrollId)
                .FirstOrDefaultAsync(cancellationToken);

            if (payrollActive is null)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    $"Ocurrio un error no puedes registrar solicitud si no existe un proceso de nomina activa asociado a este colaborador", 
                    "ERP:003"
                );                
            }

            var collaborator = await _unitOfWork.Collaborators.Entities
                .Include(col => col.WorkingInformation)
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

           DateOnly? startToValidate =
                request.PermitApplicationType == PermitApplicationType.MedicalAppointment
                    ? request.PermitApplicationMedicalAppointment?.StartDate
                    : request.PermitApplicationVacation?.StartDate;

            DateOnly? endToValidate =
                request.PermitApplicationType == PermitApplicationType.MedicalAppointment
                    ? request.PermitApplicationMedicalAppointment?.StartDate
                    : request.PermitApplicationVacation?.EndDate;

            if (startToValidate.HasValue && endToValidate.HasValue)
            {
                var (hasOverlap, overlapMessage) = await ValidateOverlapAsync(
                    collaborator.Id,
                    startToValidate.Value,
                    endToValidate.Value,
                    cancellationToken);

                if (hasOverlap)
                {
                    return _errorManager.ThrowBadRequest<bool>(
                        overlapMessage,
                        "ERP:DATE_OVERLAP");
                }
            }

            #endregion Validaciones

            var AdditionalData    = new AdditionalDataPermitApplication();
            
            var permitApplication = new PermitApplicationEntity
            {
                EndTime          = null,
                StartTime        = null,
                Status           = PermitApplicationStatus.Pending,
                CollaboratorCode = collaborator.CollaboratorCode,
                CollaboratorId   = collaborator.Id,
                Description      = request.Description,
                PayrolId         = request.PayrollId,
                RequestedBy      = ManagerUtils.FromSliceToCollaboratorFullname(collaborator)
            };

            switch (request.PermitApplicationType)
            {
                case PermitApplicationType.MedicalAppointment:
                {
                    _logger.LogInformation("🚀Iniciando proceso de registro de solicitud de permiso.");

                    var medicalReq = request?.PermitApplicationMedicalAppointment ?? new();

                    permitApplication.IsWithRangeDate = false;
                    permitApplication.StartDate = medicalReq.StartDate;
                    permitApplication.EndDate   = medicalReq.StartDate;
                    permitApplication.StartTime = medicalReq.StartTime;
                    permitApplication.EndTime   = medicalReq.EndTime;
                    permitApplication.Type      = PermitApplicationType.MedicalAppointment;

                    MappingRecords(request?.Channel ?? Channels.PersonalPanel, permitApplication, access.Role?.RoleType ?? RoleType.Operator);                    

                    if (medicalReq.IsFullDay)
                    {
                        //Lo definimos como dia completo
                        permitApplication.AmountDays = 1;
                        AdditionalData.MedicalAppointmentData.IsFullDay = true;
                    }
                    else
                    {
                        //Calcular las horas que solicito para evitar, falsos positivos
                        var duration = medicalReq.EndTime!.Value - medicalReq.StartTime!.Value;
                        decimal totalHours = (decimal) duration.TotalHours;

                        permitApplication.AmountDays = totalHours;
                        AdditionalData.MedicalAppointmentData.IsFullDay = false;
                    }

                    //Evaluar las imagenes adjuntadas.
                    AdditionalData.MedicalAppointmentData.ImagesAttached = medicalReq.Images;

                    //✅Terminar de evaluar la solicitud
                    break;
                }
                case PermitApplicationType.DonatedVacations:
                {
                    //Validamos la cedula de la persona que viene a recibir el gozo de vacaciones donadas.
                    // if (request.PermitApplicationDonatedVacations?.IdentificationCollaboratorToReceive is null)
                    // {
                    //     return _errorManager.ThrowBadRequest<bool>("La identificación del colaborador que recibiras las vacaciones es requerido!", "ERP:02"); 
                    // }

                    // var collaboratoToReceive = await _unitOfWork.Collaborators.Entities
                    //     .Where(col => col.IdentificationNumber == request.PermitApplicationDonatedVacations!.IdentificationCollaboratorToReceive)
                    //     .FirstOrDefaultAsync(cancellationToken);

                    // if (collaboratoToReceive is null)
                    // {
                    //     return _errorManager.ThrowBadRequest<bool>("El colaborador beneficiado por las vacaciones donadas no se existe!", "ERP:03");       
                    // }

                    // var vacationControl = await _unitOfWork.Vacations.Entities
                    //     .Where(vac => vac.CollaboratorId == collaborator.Id)
                    //     .FirstOrDefaultAsync(cancellationToken);

                    // if (vacationControl is null)
                    // {
                    //     return _errorManager.ThrowBadRequest<bool>("No se encontro su control de vacaciones no puede generar esta solicitud", "ERP:04");       
                    // }

                    // if (vacationControl.AvailableVacations < request.PermitApplicationDonatedVacations.AmountDays)
                    // {
                    //      return _errorManager.ThrowBadRequest<bool>("No se cuentas con sufientes dias de vacaciones para donar", "ERP:05");       
                    // }

                    // //Mapeamos la data para crear la solicitud de permiso
                    // MapperCaseDefaultValues(permitApplication, access.Role!.RoleType, request.Channel, request.ModuleCode);
                    // permitApplication.Type = PermitApplicationType.DonatedVacations;
                    // permitApplication.AmountDays = request.PermitApplicationDonatedVacations?.AmountDays ?? 0;
                    // permitApplication.IdentificationCollaboratorToReceive = request.PermitApplicationDonatedVacations?.IdentificationCollaboratorToReceive ?? string.Empty;

                    break;   
                }
                case PermitApplicationType.Vacation:
                {
                    //Registro de solicitud de vacaciones
                    var vacationData = request.PermitApplicationVacation!;

                    permitApplication.PayrolId  = request.PayrollId;
                    permitApplication.EndDate   = vacationData.EndDate;
                    permitApplication.EndTime   = vacationData.EndTime;
                    permitApplication.StartDate = vacationData.StartDate;
                    permitApplication.StartTime = vacationData.StartTime;
                    permitApplication.Type      = PermitApplicationType.Vacation;

                    var vacationControl = await _unitOfWork.Vacations.Entities
                        .Include(vtl => vtl.Collaborator)
                        .Where(vtl => vtl.Collaborator.IdentificationNumber == request.IdentificationNumber)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (vacationControl is null)
                    {
                        return _errorManager.ThrowBadRequest<bool>("No se encontro el registro de vacaciones de este colaborador", "ERP:01");
                    }

                    if (request.PermitApplicationVacation?.IsFullDay ?? false)
                    {
                        if (vacationControl.AvailableVacations < 1.0m)
                        {
                            return _errorManager.ThrowBadRequest<bool>("No cuentas con dias sufientes para solicitar vacaciones", "ERP:02");                        
                        }
                        
                        permitApplication.AmountDays = 1.0m;
                        permitApplication.IsWithRangeDate = false;
                    }
                    else if (request.PermitApplicationVacation?.IsItMidday ?? false)
                    {
                        if (vacationControl.AvailableVacations < 0.5m)
                        {
                            return _errorManager.ThrowBadRequest<bool>("No cuentas con dias sufientes para solicitar vacaciones", "ERP:03");                        
                        }

                        permitApplication.AmountDays = 0.5m;
                        permitApplication.IsWithRangeDate = false;
                    }
                    else if (request.PermitApplicationVacation?.WithRangeHours ?? false)
                    {
                        var endTime = request.PermitApplicationVacation.EndTime!.Value;
                        var startTime = request.PermitApplicationVacation.StartTime!.Value;

                        int totalHours = endTime.Hour - startTime.Hour;

                        decimal daysToDeduct = totalHours switch
                        {
                            1 => 0.1m,
                            2 => 0.2m,
                            3 => 0.3m,
                            4 => 0.4m,
                            5 => 0.5m,
                            6 => 0.6m,
                            7 => 0.7m,
                            _ when totalHours >= 8 => 1.0m,
                            _ => 0.0m
                        };

                        permitApplication.AmountDays = daysToDeduct;
                        permitApplication.IsWithRangeDate = false;
                    }
                    else
                    {
                        decimal totalDays = 0;

                        DateOnly startDate = request.PermitApplicationVacation!.StartDate;
                        DateOnly endDate   = request.PermitApplicationVacation.EndDate;

                        if (!collaborator.DoesWorkSaturdays && endDate.DayOfWeek == DayOfWeek.Friday)
                        {
                            int daysUntilSunday = (7 - (int)endDate.DayOfWeek) % 7;
                            endDate = endDate.AddDays(daysUntilSunday);
                        }
                        else if (collaborator.DoesWorkSaturdays && endDate.DayOfWeek == DayOfWeek.Saturday)
                        {
                            int daysUntilSunday = (7 - (int)endDate.DayOfWeek) % 7;
                            endDate = endDate.AddDays(daysUntilSunday);
                        }

                        var holidays = await _unitOfWork.Holidays.Entities
                            .Where(day => day.IsActive)
                            .ToListAsync(cancellationToken);

                        int totalCalendarDays = endDate.DayNumber - startDate.DayNumber + 1;
                        int fullWeeks         = totalCalendarDays / 7;
                        int remainingDays     = totalCalendarDays % 7;

                        // Semanas completas siempre son 7 días fijos
                        totalDays += fullWeeks * 7;

                        // Días sobrantes se calculan proporcional
                        for (int i = 0; i < remainingDays; i++)
                        {
                            DateOnly date = startDate.AddDays(fullWeeks * 7 + i);

                            // Domingo nunca cuenta en días parciales
                            if (date.DayOfWeek == DayOfWeek.Sunday)
                                continue;

                            bool isHoliday = holidays.Any(h =>
                                h.Day   == date.Day   &&
                                h.Month == date.Month &&
                                (
                                    h.IsGlobal ||
                                    (collaborator.WorkingInformation != null &&
                                    h.BranchId == collaborator.WorkingInformation.CompanyBranchId)
                                )
                            );

                            if (isHoliday)
                                continue;

                            if (date.DayOfWeek == DayOfWeek.Saturday)
                            {
                                if (collaborator.DoesWorkSaturdays)
                                    totalDays += 0.5m;

                                continue;
                            }

                            totalDays += 1;
                        }

                        if(vacationControl.AvailableVacations < totalDays)
                        {
                            return _errorManager.ThrowBadRequest<bool>(
                                "No cuenta con cantidad de dias suficiente para realizar esta solicitud", 
                                "ERP:04"
                            );
                        }

                        permitApplication.AmountDays = totalDays;
                        permitApplication.IsWithRangeDate = true;
                    }

                    break;
                }
                default:    
                {
                    return _errorManager.ThrowBadRequest<bool>("Este tipo de solicitud no se encuentra disponible de momento", "ERP:ErrorRequest"); 
                }
            }

            //✅Serializamos la data adicional del permiso, solicitado y registramos
            permitApplication.AdditionalData = JsonSerializer.Serialize(AdditionalData);
            await _unitOfWork.PermitApplications.CreatePermitApplication(permitApplication);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        private async Task<(bool hasOverlap, string message)> ValidateOverlapAsync(Guid collaboratorId, DateOnly startDate, DateOnly endDate, CancellationToken ct)
        {

            var overlapExists = await _unitOfWork.PermitApplications.Entities
                .AnyAsync(p =>
                    p.CollaboratorId == collaboratorId &&
                    (p.Status == PermitApplicationStatus.Approved ||
                    p.Status == PermitApplicationStatus.Pending) &&
                    startDate <= p.EndDate &&
                    endDate >= p.StartDate,
                    ct
                );

            if (overlapExists)
            {
                return (
                    true,
                    "Ya existe una solicitud (pendiente o aprobada) para las fechas seleccionadas. No se permite duplicar solicitudes el mismo día."
                );
            }

            return (false, string.Empty);
        }

        public static void MappingRecords(Channels channels, PermitApplicationEntity entity, RoleType role)
        {
            if (channels == Channels.AdministrativePanel && role == RoleType.Administrator)
            {
                entity.FirtsStepApproved = true;
                entity.ManagerFullname = "Control Administrativo";
            }
            else if(channels == Channels.PersonalPanel && role == RoleType.Manager)
            {
                entity.FirtsStepApproved = true;
                entity.ManagerFullname = "Control Administrativo";
            }
        }
    }
}   