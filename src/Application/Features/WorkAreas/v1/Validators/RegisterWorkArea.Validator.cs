using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Validators
{
    public class RegisterWorkAreaValidator: AbstractValidator<RegisterWorkAreaCommand>
    {
        public RegisterWorkAreaValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.WorkAreaName)
                .NotEmpty()
                    .WithMessage("El nombre del area de trabajo es requerido.")
                .NotNull()
                    .WithMessage("El nombre del area de trabajo es requerido.");
        }
    }
}