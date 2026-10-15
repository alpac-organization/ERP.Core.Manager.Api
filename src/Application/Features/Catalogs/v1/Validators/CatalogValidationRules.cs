using System.Linq.Expressions;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;
using FluentValidation;

internal static class CatalogValidationRules
{
    private static readonly TierPriceValidator TierFF = new(false, false);
    private static readonly TierPriceValidator TierTF = new(true, false);
    private static readonly TierPriceValidator TierFT = new(false, true);
    private static readonly TierPriceValidator TierTT = new(true, true);

    private static readonly SupplierProductItemValidator SuppProdFF = new(TierFF);
    private static readonly SupplierProductItemValidator SuppProdTF = new(TierTF);
    private static readonly SupplierProductItemValidator SuppProdFT = new(TierFT);
    private static readonly SupplierProductItemValidator SuppProdTT = new(TierTT);

    private static readonly ProductSupplierItemValidator ProdSuppFF = new(TierFF);
    private static readonly ProductSupplierItemValidator ProdSuppTF = new(TierTF);
    private static readonly ProductSupplierItemValidator ProdSuppFT = new(TierFT);
    private static readonly ProductSupplierItemValidator ProdSuppTT = new(TierTT);

    private static SupplierProductItemValidator GetSupplierProductValidator(bool reqValidFrom, bool reqValidFromNotInPast) =>
        (reqValidFrom, reqValidFromNotInPast) switch
        {
            (false, false) => SuppProdFF,
            (true, false)  => SuppProdTF,
            (false, true)  => SuppProdFT,
            (true, true)   => SuppProdTT
        };

    private static ProductSupplierItemValidator GetProductSupplierValidator(bool reqValidFrom, bool reqValidFromNotInPast) =>
        (reqValidFrom, reqValidFromNotInPast) switch
        {
            (false, false) => ProdSuppFF,
            (true, false)  => ProdSuppTF,
            (false, true)  => ProdSuppFT,
            (true, true)   => ProdSuppTT
        };

    public static void ApplySupplierIdRules<T>(AbstractValidator<T> validator, Expression<Func<T, Guid>> selector)
    {
        validator.RuleFor(selector).NotEmpty().WithMessage("El id del proveedor es requerido.");
    }

    public static void ApplyProductIdRules<T>(AbstractValidator<T> validator, Expression<Func<T, Guid>> selector)
    {
        validator.RuleFor(selector).NotEmpty().WithMessage("El id del producto es requerido.");
    }

    public static void ApplyProductSupplierItemsRules<T>(
        AbstractValidator<T> validator,
        Expression<Func<T, IEnumerable<ProductSupplierItemDto>>> selector,
        bool requireValidFrom = false,
        bool requireValidFromNotInPast = false)
    {
        validator.RuleForEach(selector).SetValidator(GetProductSupplierValidator(requireValidFrom, requireValidFromNotInPast));
    }

    public static void ApplySupplierProductItemsRules<T>(
        AbstractValidator<T> validator,
        Expression<Func<T, IEnumerable<SupplierProductItemDto>>> selector,
        bool requireValidFrom = false,
        bool requireValidFromNotInPast = false)
    {
        validator.RuleForEach(selector).SetValidator(GetSupplierProductValidator(requireValidFrom, requireValidFromNotInPast));
    }
}