using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

public class UpdateSupplierProductPriceValidator : AbstractValidator<UpdateSupplierProductPriceCommand>
{
    public UpdateSupplierProductPriceValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El id de usuario es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id de usuario no es válido.");

        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("El id de la empresa es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id de la empresa no es válido.");

        RuleFor(x => x.ModuleCode)
            .NotEmpty().WithMessage("El código de módulo es requerido.");

        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("El id del proveedor es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id del proveedor no es válido.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("El id del producto es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id del producto no es válido.");

        RuleFor(x => x)
            .Must(x => x.NewUnitPrice.HasValue || (x.TierPrices is { Count: > 0 }))
            .WithMessage("Debe indicar un nuevo precio unitario y/o precios preferenciales.");

        RuleFor(x => x.NewUnitPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.NewUnitPrice.HasValue)
            .WithMessage("El precio unitario no puede ser negativo.");

        RuleForEach(x => x.TierPrices).ChildRules(tier =>
        {
            tier.RuleFor(t => t.MinQuantity)
                .GreaterThan(0).WithMessage("La cantidad mínima debe ser mayor a cero.");

            tier.RuleFor(t => t.PreferentialPrice)
                .GreaterThanOrEqualTo(0).WithMessage("El precio preferencial no puede ser negativo.");

            tier.RuleFor(t => t)
                .Must(t => !t.ValidTo.HasValue || t.ValidTo.Value >= t.ValidFrom)
                .WithMessage("La fecha de fin de vigencia no puede ser anterior a la fecha de inicio.");
        });
    }
}
