using FluentValidation;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

public class RegisterProductValidator : BaseRequestValidator<RegisterProductCommand>
{
    public RegisterProductValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("El nombre del producto es obligatorio.")
            .MaximumLength(80).WithMessage("El nombre del producto no puede exceder los 80 caracteres.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("La categoría es obligatoria.")
            .NotEqual(Guid.Empty).WithMessage("El id de la categoría no es válido.");

        RuleFor(x => x.UnitMeasureId)
            .NotEmpty().WithMessage("La unidad de medida es obligatoria.")
            .NotEqual(Guid.Empty).WithMessage("El id de la unidad de medida no es válido.");

        RuleFor(x => x.ProductUsageType)
            .IsInEnum().WithMessage("El tipo de uso del producto es inválido.");

        CatalogValidationRules.ApplyProductSupplierItemsRules(this, x => x.Suppliers, requireValidFrom: true);
    }
}
