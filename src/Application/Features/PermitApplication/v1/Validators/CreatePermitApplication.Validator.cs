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

            RuleFor(x => x.IdentificationNumber)
                .NotEmpty().WithMessage("El número de identificación es requerido")
                .NotNull().WithMessage("El número de identificación es requerido"); 

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
        }

        #region Validar Solicitud de donación de vacaciones

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

        #endregion Validar Solicitud de donación de vacaciones

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
    }
}