using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Notifications.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Notifications.v1.Validators
{
    public class RegisterPushTokenValidator : AbstractValidator<RegisterPushTokenCommand>
    {
        public RegisterPushTokenValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                    .WithMessage("El token push es requerido.")
                .NotNull()
                    .WithMessage("El token push es requerido.");
        }
    }
}