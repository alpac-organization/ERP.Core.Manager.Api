using System.Linq.Expressions;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using FluentValidation;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

internal static class SupplierProductValidationRules
{
    public static void ApplySupplierIdRules<T>(AbstractValidator<T> validator, Expression<Func<T, Guid>> selector)
    {
        validator.RuleFor(selector).NotEmpty().WithMessage("El id del proveedor es requerido.");
    }

    public static void ApplyProductIdRules<T>(AbstractValidator<T> validator, Expression<Func<T, Guid>> selector)
    {
        validator.RuleFor(selector).NotEmpty().WithMessage("El id del producto es requerido.");
    }

    public static void ApplyProductSupplierItemsRules<T>(AbstractValidator<T> validator,Expression<Func<T, IEnumerable<ProductSupplierItemDto>>> selector,
        bool requireValidFrom = false,
        bool requireValidFromNotInPast = false,
        bool requireCoreFields = false)
    {
        validator.RuleForEach(selector).SetValidator(new ProductSupplierItemValidator(
            new TierPriceValidator(requireValidFrom, requireValidFromNotInPast, requireCoreFields)));
    }

    public static void ApplySupplierProductItemsRules<T>(AbstractValidator<T> validator,Expression<Func<T, IEnumerable<SupplierProductItemDto>>> selector,
        bool requireValidFrom = false,
        bool requireValidFromNotInPast = false,
        bool requireUnitPrice = false,
        bool requireCoreFields = false)
    {
        validator.RuleForEach(selector).SetValidator(new SupplierProductItemValidator(
            new TierPriceValidator(requireValidFrom, requireValidFromNotInPast, requireCoreFields),
            requireUnitPrice));
    }
}
