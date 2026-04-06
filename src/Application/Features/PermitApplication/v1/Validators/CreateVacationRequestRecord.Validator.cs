using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Commands;
using FluentValidation;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Validators
{
    public class CreateVacationRequestRecordValidator : AbstractValidator<CreatePermitApplicationCommand>
    {
        public CreateVacationRequestRecordValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.ModuleCode)
                .NotEmpty()
                    .WithMessage("El codigo de modulo es requerido")
                .NotNull()
                    .WithMessage("El codigo de modulo es requerido");

            RuleFor(x => x.UserId)
                .NotEmpty()
                    .WithMessage("El id de usuario es requerido")
                .NotNull()
                    .WithMessage("El id de usuario es requerido");

            RuleFor(x => x.IdentificationNumber)
                .NotEmpty()
                    .WithMessage("El número de identificación es requerido")
                .NotNull()
                    .WithMessage("El número de identificación es requerido"); 
                    
            RuleFor(x => x.StartDate)
                .NotEmpty()
                .Must(date => date.Date >= DateTime.UtcNow.Date)
                .WithMessage("La fecha de inicio no puede ser en el pasado");

            RuleFor(x => x.EndDate)
                .NotEmpty()
                .GreaterThan(x => x.StartDate)
                .WithMessage("La fecha de fin debe ser mayor que la fecha de inicio");

            RuleFor(x => x)
                .Must(x => (x.EndDate - x.StartDate).TotalDays <= 30)
                .WithMessage("No puedes solicitar más de 30 días de vacaciones");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("La descripción no puede exceder los 500 caracteres");
        }
    }
}