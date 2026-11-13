using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Validators
{
    public class GetCostCentersValidator : AbstractValidator<GetCostCentersQuery>
    {
        public GetCostCentersValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puede ser vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.ModuleCode)
                .NotEmpty()
                    .WithMessage("El codigo de modulo no puede ser vacio.")
                .NotNull()
                    .WithMessage("El codigo de modulo es requerido");

            RuleFor(x => x.UserId)
                .NotEmpty()
                    .WithMessage("El id de usuario no puede ser vacio.")
                .NotNull()
                    .WithMessage("El id de usuario es requerido");
        }
    }
}