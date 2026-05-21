using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Validators
{
    public class GetTypesSubsidyAvailableValidator: AbstractValidator<GetTypesIncomeAvailableQuery>
    {
        public GetTypesSubsidyAvailableValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");
        }
    }
}