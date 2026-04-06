using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands;
using ERP.Core.Manager.Api.Domain.Enums;
using FluentValidation;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Validators
{
    public class CreatePermitApplicationValidator : AbstractValidator<CreatePermitApplicationCommand>
    {
        public CreatePermitApplicationValidator()
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

            RuleSet("TimeValidation", () => {
                RuleFor(x => x.StartTime)
                    .NotEmpty().NotNull()
                    .WithMessage("La hora de inicio es requerida para este tipo de permiso.")
                    .When(x => x.PermitApplicationType != PermitApplicationType.Vacation && 
                               x.PermitApplicationType != PermitApplicationType.SpecialLeave);

                RuleFor(x => x.EndTime)
                    .NotEmpty().NotNull()
                    .WithMessage("La hora de fin es requerida para este tipo de permiso.")
                    .When(x => x.PermitApplicationType != PermitApplicationType.Vacation && 
                               x.PermitApplicationType != PermitApplicationType.SpecialLeave);
                
                RuleFor(x => x)
                    .Must(x => x.EndTime > x.StartTime)
                    .WithMessage("La hora de fin debe ser posterior a la hora de inicio.")
                    .When(x => x.StartTime.HasValue && x.EndTime.HasValue &&
                               x.PermitApplicationType != PermitApplicationType.Vacation && 
                               x.PermitApplicationType != PermitApplicationType.SpecialLeave);
            });
            
            RuleFor(x => x.PermitApplicationType)
                .NotNull()
                    .WithMessage("El tipo de permiso es requerido.")
                .IsInEnum()
                    .WithMessage("El tipo de permiso seleccionado no es válido.");
                    
            RuleFor(x => x.StartDate)
                .NotEmpty()
                .Must(date => date.Date >= DateTime.UtcNow.Date)
                .WithMessage("La fecha de inicio no puede ser en el pasado");

            RuleFor(x => x.EndDate)
                .NotEmpty()
                .NotNull()
                .WithMessage("La fecha de fin es obligatoria para solicitudes de vacaciones.")
                .GreaterThan(x => x.StartDate)
                .WithMessage("Las vacaciones deben durar al menos un día (la fecha de fin debe ser mayor a la de inicio).")
                .When(x => x.PermitApplicationType == PermitApplicationType.Vacation);

            RuleFor(x => x)
                .Must(x => {
                    if (!x.EndDate.HasValue) return true;
                    return (x.EndDate.Value.Date - x.StartDate.Date).TotalDays <= 30;
                })
                .WithMessage("No puedes solicitar más de 30 días de vacaciones.")
                .When(x => x.PermitApplicationType == PermitApplicationType.Vacation);


            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("La descripción no puede exceder los 500 caracteres");
        }
    }
}