using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using FluentValidation;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

public class RegisterProductValidator : AbstractValidator<RegisterProductCommand>
{
    public RegisterProductValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El id de usuario es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id de usuario no es válido");

        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("El id de la empresa es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id de la empresa no es válido.");

        RuleFor(x => x.ModuleCode)
            .NotEmpty().WithMessage("El código de módulo es requerido.");

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

        RuleForEach(x => x.Suppliers).ChildRules(supplier =>
        {
            supplier.RuleFor(s => s.SupplierId)
                .NotEmpty().WithMessage("El id del proveedor es obligatorio.")
                .NotEqual(Guid.Empty).WithMessage("El id del proveedor no es válido.");

            supplier.RuleFor(s => s.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("El precio unitario no puede ser negativo.");

            supplier.RuleForEach(s => s.TierPrices).ChildRules(tier =>
            {
                tier.RuleFor(t => t.MinQuantity)
                    .GreaterThan(0).WithMessage("La cantidad mínima debe ser mayor a cero.");

                tier.RuleFor(t => t.PreferentialPrice)
                    .GreaterThanOrEqualTo(0).WithMessage("El precio preferencial no puede ser negativo.");

                tier.RuleFor(t => t.ValidFrom)
                    .NotEmpty().WithMessage("La fecha de inicio de vigencia es obligatoria.");

                tier.RuleFor(t => t)
                    .Must(t => !t.ValidTo.HasValue || t.ValidTo.Value >= t.ValidFrom)
                    .WithMessage("La fecha de fin de vigencia no puede ser anterior a la fecha de inicio.");
            });
        });
    }
}
