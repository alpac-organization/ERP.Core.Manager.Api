using System.Linq.Expressions;
using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

internal static class CatalogValidationRules
{
    public static void ApplySupplierIdRules<T>(AbstractValidator<T> validator, Expression<Func<T, Guid>> selector)
    {
        validator.RuleFor(selector)
            .NotEmpty().WithMessage("El id del proveedor es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id del proveedor no es válido.");
    }

    public static void ApplyProductIdRules<T>(AbstractValidator<T> validator, Expression<Func<T, Guid>> selector)
    {
        validator.RuleFor(selector)
            .NotEmpty().WithMessage("El id del producto es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id del producto no es válido.");
    }

    public static void ApplyTierPriceRules(InlineValidator<TierPriceDto> tier, bool requireValidFrom = false)
    {
        tier.RuleFor(t => t.MinQuantity)
            .GreaterThan(0).WithMessage("La cantidad mínima debe ser mayor a cero.");

        tier.RuleFor(t => t.PreferentialPrice)
            .GreaterThanOrEqualTo(0).WithMessage("El precio preferencial no puede ser negativo.");

        if (requireValidFrom)
        {
            tier.RuleFor(t => t.ValidFrom)
                .NotEmpty().WithMessage("La fecha de inicio de vigencia es obligatoria.");
        }

        tier.RuleFor(t => t.UnitMeasureId)
            .NotEmpty().WithMessage("El id de la unidad de medida no es válido.")
            .When(t => t.UnitMeasureId.HasValue);

        tier.RuleFor(t => t)
            .Must(t => !t.ValidTo.HasValue || t.ValidTo.Value >= t.ValidFrom)
            .WithMessage("La fecha de fin de vigencia no puede ser anterior a la fecha de inicio.");
    }

    public static void ApplyProductSupplierItemsRules<T>(
        AbstractValidator<T> validator,
        Expression<Func<T, IEnumerable<ProductSupplierItemDto>>> selector,
        bool requireValidFrom = false)
    {
        validator.RuleForEach(selector).ChildRules(supplier =>
        {
            supplier.RuleFor(s => s.SupplierId)
                .NotEmpty().WithMessage("El id del proveedor es obligatorio.")
                .NotEqual(Guid.Empty).WithMessage("El id del proveedor no es válido.");

            supplier.RuleFor(s => s.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("El precio unitario no puede ser negativo.");

            supplier.RuleForEach(s => s.TierPrices)
                .ChildRules(tier => ApplyTierPriceRules(tier, requireValidFrom));
        });
    }

    public static void ApplySupplierProductItemsRules<T>(
        AbstractValidator<T> validator,
        Expression<Func<T, IEnumerable<SupplierProductItemDto>>> selector,
        bool requireValidFrom = false)
    {
        validator.RuleForEach(selector).ChildRules(product =>
        {
            product.RuleFor(p => p.ProductId)
                .NotEmpty().WithMessage("El id del producto es obligatorio.")
                .NotEqual(Guid.Empty).WithMessage("El id del producto no es válido.");

            product.RuleFor(p => p.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("El precio unitario no puede ser negativo.");

            product.RuleForEach(p => p.TierPrices)
                .ChildRules(tier => ApplyTierPriceRules(tier, requireValidFrom));
        });
    }
}
