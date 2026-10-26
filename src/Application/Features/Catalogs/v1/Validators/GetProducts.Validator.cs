
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
                .WithMessage("El id de la empresa es requerido.");

            RuleFor(x => x.ModuleCode)
                .NotEmpty()
                .WithMessage("El código del módulo es requerido.");

            RuleFor(x => x.ProductUsageType)
                .NotEmpty()
                .WithMessage("Los tipos de productos son obligatorio");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("El número de página debe ser mayor que 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("El tamaño de página debe ser mayor que 0.");
        }
    }
}

