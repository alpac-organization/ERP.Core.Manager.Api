using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El id de usuario es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id de usuario no es válido.");

        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("El id de la empresa es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id de la empresa no es válido.");

        RuleFor(x => x.ModuleCode)
            .NotEmpty().WithMessage("El código de módulo es requerido.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

        RuleFor(x => x.Code)
            .MaximumLength(20).WithMessage("El código no puede exceder los 20 caracteres.");

        RuleFor(x => x.ParentId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("El id del padre no es válido.");
    }
}