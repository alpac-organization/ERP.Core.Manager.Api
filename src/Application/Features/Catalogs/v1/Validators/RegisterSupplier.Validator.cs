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

            RuleFor(x => x.IdentificationType)
                .IsInEnum()
                .WithMessage("El tipo de identificación es inválido.");

            RuleFor(x => x.IdentificationType)
                .NotNull()
                .WithMessage("El tipo de identificación es obligatorio cuando se especifica el número de identificación.")
                .When(x => !string.IsNullOrWhiteSpace(x.IdentificationNumber));

            RuleFor(x => x.IdentificationNumber)
                .NotEmpty()
                .WithMessage("El número de identificación es obligatorio cuando se especifica el tipo de identificación.");

            RuleFor(x => x.IdentificationNumber)
                .MaximumLength(50)
                .WithMessage("El número de identificación no puede exceder 50 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.IdentificationNumber));

            RuleFor(x => x.ConstitutionType)
                .IsInEnum()
                .WithMessage("El tipo de constitución es inválido.");

            RuleFor(x => x.SupplierDetails)
                .NotNull()
                .WithMessage("Los detalles del proveedor son obligatorios.");

            RuleFor(x => x.SupplierDetails.CreditDays)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Los días de crédito no pueden ser negativos.");

            RuleFor(x => x.SupplierDetails.Address)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.SupplierDetails.Address));

            RuleFor(x => x.SupplierDetails.EmailSupport)
                .EmailAddress()
                .WithMessage("El correo de soporte no es válido.")
                .When(x => !string.IsNullOrWhiteSpace(x.SupplierDetails.EmailSupport));

            RuleFor(x => x.SupplierDetails.ContactName)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.SupplierDetails.ContactName));

            RuleFor(x => x.SupplierDetails.ContactEmail)
                .EmailAddress()
                .WithMessage("El correo del contacto no es válido.")
                .When(x => !string.IsNullOrWhiteSpace(x.SupplierDetails.ContactEmail));

            RuleFor(x => x.SupplierDetails.ContactPhoneNumber)
                .Matches(@"^(\+505[\s-]?)?\d{4}[\s-]?\d{4}$")
                .WithMessage("El número de teléfono debe tener 8 dígitos, opcionalmente con +505")
                .When(x => !string.IsNullOrWhiteSpace(x.SupplierDetails.ContactPhoneNumber));
        }
    }
}