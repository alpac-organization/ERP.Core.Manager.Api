using FluentValidation;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators
{
    public class RegisterSupplierValidator : BaseRequestValidator<RegisterSupplierCommand>
    {
        public RegisterSupplierValidator()
        {
            RuleFor(x => x.SuppliersLegalName)
                .NotEmpty()
                .MaximumLength(200)
                .WithMessage("El nombre legal del proveedor es obligatorio.");

            RuleFor(x => x.CommercialName)
                .MaximumLength(200)
                .WithMessage("El nombre comercial no puede exceder 200 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.CommercialName));

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

            RuleFor(x => x.SupplierDetails.ExclusiveStatus)
                .Must(status => status is SupplierExclusiveStatus.None or SupplierExclusiveStatus.PendingReview)
                .WithMessage("Al registrar un proveedor solo se permite ExclusiveStatus None o PendingReview.");

            RuleFor(x => x.SupplierDetails.ExclusiveBrandsOrParts)
                .MaximumLength(500)
                .WithMessage("Las marcas o líneas exclusivas no pueden exceder 500 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.SupplierDetails.ExclusiveBrandsOrParts));

            CatalogValidationRules.ApplySupplierProductItemsRules(this, x => x.Products);

            RuleFor(x => x.SupplierDetails.CreditLimit)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El límite de crédito no puede ser negativo.")
                .When(x => x.SupplierDetails.CreditLimit.HasValue);

            RuleFor(x => x.SupplierDetails.CreditCurrency)
                .IsInEnum()
                .WithMessage("La moneda de crédito es inválida.")
                .When(x => x.SupplierDetails.CreditCurrency.HasValue);

            RuleFor(x => x)
                .Must(x => ResolveSupplierType(x).HasValue)
                .WithMessage("El tipo de proveedor es obligatorio (supplier_type o supplier_details.supplier_type).")
                .OverridePropertyName("supplier_type");

            RuleFor(x => x)
                .Must(x =>
                {
                    var supplierType = ResolveSupplierType(x);
                    return supplierType is null || Enum.IsDefined(typeof(SupplierType), supplierType.Value);
                })
                .WithMessage("El tipo de proveedor es inválido.")
                .OverridePropertyName("supplier_type")
                .When(x => ResolveSupplierType(x).HasValue);

            RuleFor(x => x.SupplierDetails.Currency)
                .IsInEnum()
                .WithMessage("La moneda del proveedor es inválida.");

            RuleFor(x => x.SupplierDetails.AlertDaysBeforeDue)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Los días de alerta no pueden ser negativos.");

            RuleFor(x => x.PaymentMethods)
                .NotNull()
                .WithMessage("Los métodos de pago son obligatorios.")
                .Must(list => list.Count > 0)
                .WithMessage("Debe especificar al menos un método de pago.");

            RuleForEach(x => x.PaymentMethods)
                .IsInEnum()
                .WithMessage("El método de pago es inválido.");

            RuleFor(x => x.BankAccounts)
                .Must(list => list.Count(b => b.IsPrimary) <= 1)
                .WithMessage("Solo se puede marcar una cuenta bancaria como principal.")
                .When(x => x.BankAccounts != null && x.BankAccounts.Count > 0);

            RuleForEach(x => x.BankAccounts).ChildRules(account =>
            {
                account.RuleFor(b => b.BankName)
                    .NotEmpty().WithMessage("El nombre del banco es obligatorio.")
                    .MaximumLength(100).WithMessage("El nombre del banco no puede exceder 100 caracteres.");

                account.RuleFor(b => b.AccountNumber)
                    .NotEmpty().WithMessage("El número de cuenta es obligatorio.")
                    .MaximumLength(50).WithMessage("El número de cuenta no puede exceder 50 caracteres.");

                account.RuleFor(b => b.AccountType)
                    .IsInEnum().WithMessage("El tipo de cuenta bancaria es inválido.");

                account.RuleFor(b => b.Currency)
                    .IsInEnum().WithMessage("La moneda de la cuenta bancaria es inválida.");

                account.RuleFor(b => b.AccountHolderName)
                    .NotEmpty().WithMessage("El nombre del titular de la cuenta es obligatorio.")
                    .MaximumLength(200).WithMessage("El nombre del titular no puede exceder 200 caracteres.");

                account.RuleFor(b => b.AccountHolderIdentification)
                    .MaximumLength(50).WithMessage("La identificación del titular no puede exceder 50 caracteres.")
                    .When(b => !string.IsNullOrWhiteSpace(b.AccountHolderIdentification));
            });
        }

        private static SupplierType? ResolveSupplierType(RegisterSupplierCommand command) =>
            command.SupplierType ?? command.SupplierDetails?.SupplierType;
    }
}
