
using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators
{
    public class GetProductsValidator : AbstractValidator<GetProductsQuery>
    {
        public GetProductsValidator()
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

            RuleFor(x => x.ProductId)
                .NotEqual(Guid.Empty)
                .When(x => x.ProductId.HasValue)
                .WithMessage("El id del producto no es válido."); ;
        }
    }
}

