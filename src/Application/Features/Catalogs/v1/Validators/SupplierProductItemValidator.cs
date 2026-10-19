using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using FluentValidation;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators
{
public class SupplierProductItemValidator : AbstractValidator<SupplierProductItemDto>
{
    public SupplierProductItemValidator(TierPriceValidator tierValidator, bool requireUnitPrice = false)
    {
        RuleFor(p => p.ProductId)
            .NotEmpty()
            .WithMessage("El id del producto es obligatorio.");

        if (requireUnitPrice)
        {
            RuleFor(p => p.UnitPrice)
                .NotNull()
                .WithMessage("El precio unitario es obligatorio.");
        }

        RuleFor(p => p.UnitPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El precio unitario no puede ser negativo.")
            .When(p => p.UnitPrice.HasValue);

        RuleFor(p => p.UnitMeasureId)
            .NotEmpty()
            .WithMessage("El id de la unidad de medida no es válido.")
            .When(p => p.UnitMeasureId.HasValue);

        RuleFor(p => p.Currency).IsInEnum().WithMessage("La moneda del vínculo no es válida.")
            .When(p => p.Currency.HasValue);
        
        RuleFor(p => p.TierPrices)
            .Must(list => list is { Count: <= 1 })
            .WithMessage("Solo se permite un precio preferencial por vínculo proveedor-producto.")
            .When(p => p.TierPrices is not null);

        RuleForEach(p => p.TierPrices).SetValidator(tierValidator);
     }
   }
}