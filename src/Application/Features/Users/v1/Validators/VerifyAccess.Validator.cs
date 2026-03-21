using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Users.v1.Validators
{
    public class VerifyAccessValidator : AbstractValidator<VerifyAccessCommand>
    {
        public VerifyAccessValidator()
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
                    .WithMessage("El id del usuario es requerido")
                .NotNull()
                    .WithMessage("El id del usuario es requerido");

        }
    }
}