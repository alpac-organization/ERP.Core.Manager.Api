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
            .Must(x => x.NewUnitPrice.HasValue || (x.TierPrices is { Count: > 0 }))
            .WithMessage("Debe indicar un nuevo precio unitario y/o precios preferenciales.");

        RuleFor(x => x.NewUnitPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.NewUnitPrice.HasValue)
            .WithMessage("El precio unitario no puede ser negativo.");

        RuleForEach(x => x.TierPrices).ChildRules(tier =>
        {
            tier.RuleFor(t => t.MinQuantity)
                .GreaterThan(0).WithMessage("La cantidad mínima debe ser mayor a cero.");

            tier.RuleFor(t => t.PreferentialPrice)
                .GreaterThanOrEqualTo(0).WithMessage("El precio preferencial no puede ser negativo.");

            tier.RuleFor(t => t)
                .Must(t => !t.ValidTo.HasValue || t.ValidTo.Value >= t.ValidFrom)
                .WithMessage("La fecha de fin de vigencia no puede ser anterior a la fecha de inicio.");
        });
    }
}
