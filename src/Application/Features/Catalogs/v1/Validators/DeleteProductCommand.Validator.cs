using FluentValidation;
using ERP.Core.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators
{
    public class DeleteProductValidator : BaseRequestValidator<DeleteProductCommand>
    {
        public DeleteProductValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("El id del producto es obligatorio");
        }
    }
}
