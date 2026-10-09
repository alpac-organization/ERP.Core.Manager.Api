using FluentValidation;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

public class UpdateSupplierProductPriceValidator : BaseRequestValidator<UpdateSupplierProductPriceCommand>
{
    public UpdateSupplierProductPriceValidator()
    {
        CatalogValidationRules.ApplySupplierIdRules(this, x => x.SupplierId);
        CatalogValidationRules.ApplyProductIdRules(this, x => x.ProductId);

        RuleFor(x => x)
            .Must(x => x.NewUnitPrice.HasValue || x.Currency.HasValue || (x.TierPrices is { Count: > 0 }))
            .WithMessage("Debe indicar un nuevo precio unitario, moneda y/o precios preferenciales.");

        RuleFor(x => x.NewUnitPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.NewUnitPrice.HasValue)
            .WithMessage("El precio unitario no puede ser negativo.");

        RuleFor(x => x.Currency)
            .IsInEnum()
            .When(x => x.Currency.HasValue)
            .WithMessage("La moneda no es válida.");

        RuleForEach(x => x.TierPrices).ChildRules(tier => CatalogValidationRules.ApplyTierPriceRules(tier));
    }
}
