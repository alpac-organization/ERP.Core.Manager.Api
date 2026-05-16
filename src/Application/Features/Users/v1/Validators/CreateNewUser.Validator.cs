using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Users.v1.Validators
{
    public class CreateNewUserValidator : AbstractValidator<CreateNewUserCommand>
    {
        public CreateNewUserValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.IdentificationNumber)
                .NotEmpty()
                    .WithMessage("El número de identificación es requerido")
                .NotNull()
                    .WithMessage("El número de identificación es requerido");

            RuleFor(x => x.FullName)
                .NotEmpty()
                    .WithMessage("Debe indicar almenos un nombre y un apellido")
                .NotNull()
                    .WithMessage("Debe indicar almenos un nombre y un apellido");

            RuleFor(x => x.UserType)
                .NotEmpty()
                    .WithMessage("El tipo de usuario es requerido")
                .NotNull()
                    .WithMessage("El tipo de usuario es requerido");

            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithMessage("La contraseña no puede ser vacia")
                .MinimumLength(8)
                    .WithMessage("La contraseña debe tener al menos 8 caracteres.")
                .MaximumLength(32)
                    .WithMessage("La contraseña no puede exceder los 32 caracteres.")
                .Matches(@"[A-Z]")
                    .WithMessage("La contraseña debe contener al menos una letra mayúscula.")
                .Matches(@"[a-z]")
                    .WithMessage("La contraseña debe contener al menos una letra minúscula.")
                .Matches(@"[0-9]")
                    .WithMessage("La contraseña debe contener al menos un número.")
                .Matches(@"[^a-zA-Z0-9]")
                    .WithMessage("La contraseña debe contener al menos un carácter especial (ej: !@#$%^&*).")
                .NotEqual(x => x.Email)
                    .WithMessage("La contraseña no puede ser igual al correo.");

            RuleFor(x => x.Email)
                .EmailAddress()
                    .WithMessage("El formato del correo electrónico no es válido.")
                .MaximumLength(100)
                    .WithMessage("El correo no puede exceder los 100 caracteres.");
        }
    }
}