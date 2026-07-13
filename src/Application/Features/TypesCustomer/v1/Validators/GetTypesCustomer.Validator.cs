using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.TypesCustomer.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.TypesCustomer.v1.Validators
{
    public class GetTypesCustomerValidator : AbstractValidator<GetTypesCustomerQuery>
    {
        public GetTypesCustomerValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");
        }
    }
}