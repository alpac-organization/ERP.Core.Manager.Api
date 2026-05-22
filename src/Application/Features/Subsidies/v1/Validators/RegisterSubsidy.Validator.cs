using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Validators
{
    public class RegisterSubsidyValidator : AbstractValidator<RegisterSubsidyCommmand>
    {
        public RegisterSubsidyValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El id de usario es requerido.")
                .NotNull().WithMessage("El id de usuario es requerido.");

            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("El id de la empresa es requerido.")
                .NotNull().WithMessage("El id de la empresa es requerido.");

            RuleFor(x => x.CollaboratorId)
                .NotEmpty().WithMessage("El id del colaborador es requerido")
                .NotNull().WithMessage("El id del colaborador es requerido");

            RuleFor(x => x.ModuleCode)
                .NotEmpty().WithMessage("El codigo del modulo")
                .NotNull().WithMessage("El id del colaborador es requerido");

            RuleFor(x => x.TypeSubsidyId)
                .NotEmpty().WithMessage("El tipo de subsidio es requerido")
                .NotNull().WithMessage("El tipo de subsidio es requerido");

            RuleFor(x => x.ReferenceNumber)
                .NotEmpty().WithMessage("El número de referencia es obligatorio")
                .NotNull().WithMessage("El número de boleta es obligatorio");

            RuleFor(x => x.PayrollId)
                .NotEmpty().WithMessage("El id del periodo es obligatorio")
                .NotNull().WithMessage("El id del periodo es obligatorio");

            RuleFor(x => x.StartDate)
                .NotNull()
                    .WithMessage("La fecha de inicio del subsidio es requerida.")
                .NotEmpty()
                    .WithMessage("La fecha de inicio del subsidio es requerida.")
                .LessThanOrEqualTo(DateTime.UtcNow.Date.AddYears(1))
                    .WithMessage("La fecha de inicio del subsidio no es válida.");

            RuleFor(x => x.EndDate)
                .NotNull()
                    .WithMessage("La fecha de regreso del subsidio es requerida.")
                .NotEmpty()
                    .WithMessage("La fecha de regreso del subsidio es requerida.")
                .GreaterThan(x => x.StartDate)
                    .WithMessage("La fecha de regreso debe ser mayor a la fecha de inicio del subsidio.")
                .LessThanOrEqualTo(DateTime.UtcNow.Date.AddYears(1))
                    .WithMessage("La fecha de regreso no es válida.");

            RuleFor(x => x)
                .Must(x => (x.EndDate - x.StartDate).TotalDays <= 30)
                .WithMessage("El subsidio no puede exceder un período de 30 días.");

        }
    }
}
