using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators
{
    public class RegisterSupplierValidator : AbstractValidator<RegisterSupplierCommand>
    {
        public RegisterSupplierValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El id de usario es requerido.")
                .NotNull().WithMessage("El id de usuario es requerido.");

            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("El id de la empresa es requerido")
                .NotNull().WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.ModuleCode)
                .NotEmpty().WithMessage("El codigo de modulo es requerido")
                .NotNull().WithMessage("El codigo de modulo es requerido");

            RuleFor(x => x.SuppliersLegalName)
                .NotEmpty()
                .MaximumLength(200)
                .WithMessage("El nombre legal del proveedor es obligatorio.");

            RuleFor(x => x.IdentificationNumber)
                .NotEmpty()
                .MaximumLength(50)
                .WithMessage("El número de identificación es obligatorio.");

            RuleFor(x => x.ConstitutionType)
                .IsInEnum()
                .WithMessage("El tipo de constitución es inválido.");

            RuleFor(x => x.IdentificationType)
                .IsInEnum()
                .WithMessage("El tipo de identificación es inválido.");

            RuleFor(x => x.Address)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Address));

            RuleFor(x => x.EmailSupport)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.EmailSupport))
                .WithMessage("El correo de soporte no es válido.");

            RuleFor(x => x.ContactName)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.ContactName));

            RuleFor(x => x.ContactEmail)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail))
                .WithMessage("El correo del contacto no es válido.");

            RuleFor(x => x.ContactPhoneNumber)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.ContactPhoneNumber));
        }
    }
}
