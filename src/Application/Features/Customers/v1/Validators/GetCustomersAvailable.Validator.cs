using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Validators
{
    public class GetCustomersAvailableValitor: AbstractValidator<GetCustomersAvailableQuery>
    {
        public GetCustomersAvailableValitor()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");
        }
    }
}