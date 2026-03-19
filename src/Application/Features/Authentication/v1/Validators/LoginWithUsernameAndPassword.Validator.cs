using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Authentication.v1.Validators
{
    public class LoginWithUsernameAndPasswordValidator : AbstractValidator<LoginWithUsernameAndPasswordCommand>
    {
        public LoginWithUsernameAndPasswordValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("El id de la empresa no puede estar vacío.")
                .NotNull().WithMessage("El id de la empresa es requerido.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña no puede estar vacía.")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El nombre de usuario es requerido.")
                .MaximumLength(100).WithMessage("El usuario no puede exceder los 100 caracteres.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido.")
                .MaximumLength(100).WithMessage("El correo no puede exceder los 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Email)); 

            RuleFor(x => x.SessionDetails)
                .NotNull().WithMessage("Los detalles de la sesión son obligatorios.");

            RuleFor(x => x.SessionDetails!.DeviceName)
                .NotEmpty().WithMessage("El nombre del dispositivo no pudo ser identificado.")
                .When(x => x.SessionDetails != null);
        }
    }
}