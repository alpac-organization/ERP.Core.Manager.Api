using ERP.Core.Manager.Api.Application.Features.CategoryProducts.v1.Queries;
using FluentValidation;

namespace ERP.Core.Manager.Api.Application.Features.CategoryProducts.v1.Validatos;

public class GetCategoryProductsTreeValidator : AbstractValidator<GetCategoryProductsTreeQuery>
{
    public GetCategoryProductsTreeValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El identificador de usuario es obligatorio.")
            .NotEqual(Guid.Empty).WithMessage("El identificador de usuario no es válido.");

        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("El id de la empresa no puede estar vacío.")
            .NotEmpty()
            .WithMessage("El id de la empresa es requerido.");
    
        RuleFor(x => x.ModuleCode)
            .NotEmpty()
            .WithMessage("El codigo del módulo no puede estar vacío.")
            .NotEmpty()
            .WithMessage("El código del módulo es requerido.");
    }
}