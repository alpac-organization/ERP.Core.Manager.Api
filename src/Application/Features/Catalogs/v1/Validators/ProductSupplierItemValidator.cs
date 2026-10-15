using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using FluentValidation;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators
{
public class ProductSupplierItemValidator : AbstractValidator<ProductSupplierItemDto>
{
    public ProductSupplierItemValidator(TierPriceValidator tierValidator)
    {
        RuleFor(s => s.SupplierId)
         .NotEmpty()
         .WithMessage("El id del proveedor es obligatorio.");

        RuleFor(s => s.UnitPrice)
         .GreaterThanOrEqualTo(0)
         .WithMessage("El precio unitario no puede ser negativo.");
         
        RuleFor(s => s.UnitMeasureId)
         .NotEmpty()
         .WithMessage("El id de la unidad de medida no es válido.")
         .When(s => s.UnitMeasureId.HasValue);

        RuleFor(s => s.Currency)
        .IsInEnum()
        .WithMessage("La moneda del vínculo no es válida.")
        .When(s => s.Currency.HasValue);

        RuleFor(s => s.TierPrices)
            .Must(list => list is { Count: <= 1 })
            .WithMessage("Solo se permite un precio preferencial por vínculo proveedor-producto.")
            .When(s => s.TierPrices is not null);

        RuleForEach(s => s.TierPrices).SetValidator(tierValidator);
     }
   }
}