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
            .Must(x =>
                x.NewUnitPrice.HasValue ||
                x.Currency.HasValue ||
                x.UnitMeasureId.HasValue ||
                x.TierPrices is not null)
            .WithMessage("Debe indicar un nuevo precio unitario, moneda, unidad de medida y/o precios preferenciales.");

        RuleFor(x => x.NewUnitPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.NewUnitPrice.HasValue)
            .WithMessage("El precio unitario no puede ser negativo.");

        RuleFor(x => x.Currency)
            .IsInEnum()
            .When(x => x.Currency.HasValue)
            .WithMessage("La moneda no es válida.");

        RuleFor(x => x.UnitMeasureId)
            .NotEmpty()
            .When(x => x.UnitMeasureId.HasValue)
            .WithMessage("El id de la unidad de medida no es válido.");

        RuleFor(x => x.TierPrices)
            .Must(list => list is null || list.Count <= 1)
            .WithMessage("Solo se permite un precio preferencial por vínculo proveedor-producto.");

        RuleForEach(x => x.TierPrices)
            .ChildRules(tier => CatalogValidationRules.ApplyTierPriceRules(
                tier,
                requireValidFrom: true,
                requireValidFromNotInPast: true));
    }
}
