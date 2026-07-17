using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators
{
    public class GetCategoryProductsTreeValidator : AbstractValidator<GetCategoryProductsQuery>
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
                
            RuleFor(x => x.ParentId)
                .NotEqual(Guid.Empty)
                .When(x => x.ParentId.HasValue)
                .WithMessage("El id del padre no es válido.");;
        }
    }
}

