using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Validators
{
    public class GetTypesIncomeAvailableValidator: AbstractValidator<GetTypesIncomeAvailableQuery>
    {
        public GetTypesIncomeAvailableValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");
                    
            RuleFor(x => x.UserId)
                .NotEmpty()
                    .WithMessage("El usuario es requerido")
                .NotNull()
                    .WithMessage("El usuario es requerido");
        }
    }
}