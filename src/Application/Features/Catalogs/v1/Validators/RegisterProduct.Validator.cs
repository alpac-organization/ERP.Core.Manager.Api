using FluentValidation;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

public class RegisterProductValidator : BaseRequestValidator<RegisterProductCommand>
{
    public RegisterProductValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("El nombre del producto es obligatorio.")
            .MaximumLength(80).WithMessage("El nombre del producto no puede exceder los 80 caracteres.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("La categoría es obligatoria.");

        RuleFor(x => x.UnitMeasureId)
            .NotEmpty().WithMessage("La unidad de medida es obligatoria.");

        RuleFor(x => x.ProductUsageType)
            .IsInEnum().WithMessage("El tipo de uso del producto es inválido.");

        RuleFor(x => x.ProductUsageType)
            .IsInEnum().WithMessage("El tipo de uso del producto es inválido.");

        When(x => x.ProductUsageType == ProductUsageType.Operational, () =>
        {
            RuleFor(x => x.ConfigurationStock)
                .NotNull()
                .WithMessage("La configuración de stock es obligatoria para productos operativos.");

            RuleFor(x => x.ConfigurationStock!.MinimumStock)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El stock mínimo no puede ser negativo.")
                .When(x => x.ConfigurationStock is not null);

            RuleFor(x => x.ConfigurationStock!.MaximumStock)
                .GreaterThanOrEqualTo(x => x.ConfigurationStock!.MinimumStock)
                .WithMessage("El stock máximo no puede ser menor que el stock mínimo.")
                .When(x => x.ConfigurationStock is not null);
        });
        
        SupplierProductValidationRules.ApplyProductSupplierItemsRules(
            this,
            x => x.Suppliers,
            requireValidFrom: true,
            requireValidFromNotInPast: true,
            requireCoreFields: true);
    }
}
