using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Validators
{
    public class RegisterJobPositionValidator: AbstractValidator<RegisterJobPositionCommand>
    {
        public RegisterJobPositionValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.ModuleCode)
                .NotEmpty()
                    .WithMessage("El codigo del modulo es requerido")
                .NotNull()
                    .WithMessage("El codigo del modulo es requerido");

            RuleFor(x => x.UserId)
                .NotEmpty()
                    .WithMessage("El id de usuario es requerido")
                .NotNull()
                    .WithMessage("El id de usuario es requerido");

            RuleFor(x => x.JobPositionName)
                .NotEmpty()
                    .WithMessage("Asegurese de ingresar el nombre del cargo es obligatorio.")
                .NotNull()
                    .WithMessage("Asegurese de ingresar el nombre del cargo es obligatorio.");
                    
            RuleFor(x => x.Description)
                .MaximumLength(250)
                    .WithMessage("La descripción no puede exceder los 250 caracteres.")
                .When(x => x.Description != null);
        }
    }
}