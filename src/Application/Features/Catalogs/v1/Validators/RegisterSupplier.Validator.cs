using FluentValidation;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

public class RegisterSupplierValidator : BaseRequestValidator<RegisterSupplierCommand>
{
    private static readonly SupplierDetailsValidator DetailsValidator = new();
    private static readonly BankAccountValidator BankValidator = new();

    public RegisterSupplierValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.SuppliersLegalName)
            .NotEmpty().WithMessage("El nombre legal del proveedor es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.CommercialName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.CommercialName));

        RuleFor(x => x.IdentificationNumber)
            .MaximumLength(50).WithMessage("El número de identificación no puede exceder 50 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.IdentificationNumber));

        RuleFor(x => x.IdentificationType)
            .IsInEnum().WithMessage("El tipo de identificación es inválido.");

        RuleFor(x => x.ConstitutionType)
            .IsInEnum().WithMessage("El tipo de constitución es inválido.");

        RuleFor(x => x.SupplierType ?? x.SupplierDetails.SupplierType)
            .NotNull().WithMessage("El tipo de proveedor es obligatorio.")
            .IsInEnum().WithMessage("El tipo de proveedor es inválido.")
            .OverridePropertyName("supplier_type");

        RuleFor(x => x.SupplierDetails)
            .NotNull().WithMessage("Los detalles del proveedor son obligatorios.")
            .SetValidator(DetailsValidator); 

        RuleForEach(x => x.PaymentMethods)
            .IsInEnum().WithMessage("El método de pago es inválido.");

        RuleFor(x => x.BankAccounts)
            .Must(list => list.Count(b => b.IsPrimary) <= 1)
            .WithMessage("Solo se puede marcar una cuenta bancaria como principal.")
            .When(x => x.BankAccounts is { Count: > 1 });

        RuleForEach(x => x.BankAccounts)
            .SetValidator(BankValidator); 

        CatalogValidationRules.ApplySupplierProductItemsRules(
            this,
            x => x.Products,
            requireValidFrom: true,
            requireValidFromNotInPast: true,
            requireUnitPrice: true);
    }
}