using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Validators
{
    public class CreatePermitApplicationValidator : AbstractValidator<CreatePermitApplicationCommand>
    {
        public CreatePermitApplicationValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("El id de la empresa no puede estar vacío.")
                .NotNull().WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.ModuleCode)
                .NotEmpty().WithMessage("El código de módulo es requerido")
                .NotNull().WithMessage("El código de módulo es requerido");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El id de usuario es requerido")
                .NotNull().WithMessage("El id de usuario es requerido");

            RuleFor(x => x.PayrollId)
                .NotEmpty().WithMessage("El payroll id es requerido")
                .NotNull().WithMessage("El payroll id es requerido");

            RuleFor(x => x.IdentificationNumber)
                .NotEmpty().WithMessage("El número de identificación es requerido")
                .NotNull().WithMessage("El número de identificación es requerido");

            RuleFor(x => x.Channel)
                .NotEmpty().WithMessage("El canal es requerido.")
                .NotNull().WithMessage("El canal es requerido.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("La descripción no puede exceder los 500 caracteres");

            RuleFor(x => x.PermitApplicationType)
                .NotNull().WithMessage("El tipo de permiso es requerido.")
                .IsInEnum().WithMessage("El tipo de permiso seleccionado no es válido.");

            When(x => x.PermitApplicationType == PermitApplicationType.MedicalAppointment, () =>
            {
                RuleFor(x => x.PermitApplicationMedicalAppointment)
                    .NotNull().WithMessage("Los datos de la solicitud de donación son obligatorios.")
                    .SetValidator(new PermitApplicationMedicalAppointmentValidator());
            });

            When(x => x.PermitApplicationType == PermitApplicationType.DonatedVacations, () =>
            {
                RuleFor(x => x.PermitApplicationDonatedVacations)
                    .NotNull().WithMessage("Los datos de la solicitud de donación son obligatorios.")
                    .SetValidator(new PermitApplicationDonatedVacationsValidator());
            });

            When(x => x.PermitApplicationType == PermitApplicationType.Vacation, () =>
            {
                RuleFor(x => x.PermitApplicationVacation)
                    .NotNull().WithMessage("Los datos de la solicitud de vacaciones son obligatorios")
                    .SetValidator(new PermitApplicationVacationValidator());
            });
        }

        public class PermitApplicationDonatedVacationsValidator : AbstractValidator<PermitApplicationDonatedVacations?>
        {
            public PermitApplicationDonatedVacationsValidator()
            {
                RuleFor(x => x!.AmountDays)
                    .NotEmpty().WithMessage("La cantidad de días a donar es requerida.")
                    .GreaterThan(0).WithMessage("La cantidad de días a donar debe ser mayor a 0.");

                RuleFor(x => x!.IdentificationCollaboratorToReceive)
                    .NotEmpty().WithMessage("La identificación del colaborador que recibirá las vacaciones es requerida.");
            }
        }

        public class PermitApplicationMedicalAppointmentValidator : AbstractValidator<PermitApplicationMedicalAppointment?>
        {
            public PermitApplicationMedicalAppointmentValidator()
            {
                RuleFor(x => x!.IsFullDay)
                    .NotNull()
                    .WithMessage("Debe especificar si la cita médica es por el día completo.");

                RuleFor(x => x!.StartDate)
                    .NotEmpty()
                    .WithMessage("El día de la cita médica es obligatorio.");

                RuleFor(x => x!.StartTime)
                    .NotEmpty()
                    .When(x => x != null && !x.IsFullDay)
                    .WithMessage("La hora de inicio es requerida cuando no es un día completo.");

                RuleFor(x => x!.EndTime)
                    .NotEmpty()
                        .When(x => x != null && !x.IsFullDay)
                        .WithMessage("La hora de finalización es requerida cuando no es un día completo.")
                    .GreaterThan(x => x!.StartTime)
                        .When(x => x != null && x.StartTime != null && !x.IsFullDay)
                        .WithMessage("La hora de finalización debe ser posterior a la hora de inicio.");
            }
        }

        public class PermitApplicationVacationValidator : AbstractValidator<PermitApplicationVacation?>
        {
            public PermitApplicationVacationValidator()
            {
                RuleFor(x => x).NotNull().WithMessage("La información de la solicitud es obligatoria.");

                When(x => x != null, () =>
                {
                    RuleFor(x => x!.IsFullDay)
                        .NotNull().WithMessage("Debe especificar si es el día completo");

                    RuleFor(x => x!.WithRangeHours)
                        .NotNull().WithMessage("Debe especificar si es con rango de horas");

                    RuleFor(x => x!.IsItMidday)
                        .NotNull().WithMessage("Debe especificar si solo es medio día");

                    RuleFor(x => x!.StartDate)
                        .NotEmpty().WithMessage("La fecha de inicio es obligatoria.");

                    RuleFor(x => x!.StartTime)
                        .NotEmpty().WithMessage("La hora de inicio es obligatoria cuando se solicita por rango de horas")
                        .When(x => x!.WithRangeHours);

                    RuleFor(x => x!.EndTime)
                        .NotEmpty().WithMessage("La hora de fin es obligatoria cuando se solicita por rango de horas")
                        .When(x => x!.WithRangeHours);

                    RuleFor(x => x!.EndTime)
                        .Must((request, endTime) => endTime > request!.StartTime)
                        .WithMessage("La hora de fin debe ser mayor a la hora de inicio")
                        .When(x => x!.WithRangeHours && x.StartTime.HasValue && x.EndTime.HasValue);
                    
                    RuleFor(x => x!.StartTime)
                        .Must(t => t!.Value.Minute == 0)
                        .WithMessage("La hora de inicio debe ser una hora exacta (ej. 08:00)")
                        .When(x => x!.WithRangeHours && x.StartTime.HasValue);

                    RuleFor(x => x!.EndTime)
                        .Must(t => t!.Value.Minute == 0)
                        .WithMessage("La hora de fin debe ser una hora exacta (ej. 14:00)")
                        .When(x => x!.WithRangeHours && x.EndTime.HasValue);
                });
            }
        }
    }
}