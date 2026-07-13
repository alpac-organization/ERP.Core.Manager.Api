using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Branches.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Branches.v1.Validators
{
    public class GetBranchesAvailableValidator : AbstractValidator<GetBranchesAvailableQuery>
    {
        public GetBranchesAvailableValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("El id de la empresa es requerido.")
                .NotNull().WithMessage("El id de la empresa es requerido.");
        }
    }
}