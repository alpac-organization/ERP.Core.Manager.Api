using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El id de usuario es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id de usuario no es válido.");

        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("El id de la empresa es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id de la empresa no es válido.");

        RuleFor(x => x.ModuleCode)
            .NotEmpty().WithMessage("El código de módulo es requerido.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("El id del producto es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id del producto no es válido.");

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
    }
}
