using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Notifications.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Notifications.v1.Validators
{
    public class RegisterPushTokenValidator : AbstractValidator<RegisterPushTokenCommand>
    {
        public RegisterPushTokenValidator()
        {

            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("El id de la empresa no puede estar vacío.")
                .NotNull().WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El id de usuario es requerido")
                .NotNull().WithMessage("El id de usuario es requerido");   

            RuleFor(x => x.Token)
                .NotEmpty()
                    .WithMessage("El token push es requerido.")
                .NotNull()
                    .WithMessage("El token push es requerido.");
        }
    }
}