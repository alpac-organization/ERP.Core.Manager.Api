using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Validators
{
    public class GetCostCentersValidator: AbstractValidator<GetCostCenterQuery>
    {
        public GetCostCentersValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.AreaId)
                .NotEmpty()
                    .WithMessage("Debes seleccionar un area, es obligatorio.")
                .NotNull()
                    .WithMessage("Debes seleccionar un area, es obligatorio.");
        }
    }
}