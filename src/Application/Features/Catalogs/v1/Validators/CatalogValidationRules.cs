using System.Linq.Expressions;
using FluentValidation;

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
}
