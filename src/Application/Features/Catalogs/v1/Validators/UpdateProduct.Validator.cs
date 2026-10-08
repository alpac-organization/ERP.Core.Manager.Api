using FluentValidation;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

public class UpdateProductValidator : BaseRequestValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        CatalogValidationRules.ApplyProductIdRules(this, x => x.ProductId);

        RuleFor(x => x.ProductName)
            .MaximumLength(80)
            .When(x => !string.IsNullOrWhiteSpace(x.ProductName));

        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty)
            .When(x => x.CategoryId.HasValue)
            .WithMessage("El id de la categoría no es válido.");

        RuleFor(x => x.UnitMeasureId)
            .NotEqual(Guid.Empty)
            .When(x => x.UnitMeasureId.HasValue)
            .WithMessage("El id de la unidad de medida no es válido.");

        RuleFor(x => x.ProductUsageType)
            .IsInEnum()
            .When(x => x.ProductUsageType.HasValue);

        CatalogValidationRules.ApplyProductSupplierItemsRules(this, x => x.Suppliers);
    }
}
