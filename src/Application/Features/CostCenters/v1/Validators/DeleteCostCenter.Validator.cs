using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Validators
{
    public class DeleteCostCenterValidator: AbstractValidator<DeleteCostCenterCommand>
    {
        public DeleteCostCenterValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.CostCenterId)
                .NotEmpty()
                    .WithMessage("El id del centro de costo es requerido.")
                .NotNull()
                    .WithMessage("El id del centro de costo es requerido.");

            RuleFor(x => x.AreaId)
                .NotEmpty()
                    .WithMessage("El area al que esta asociada el centro de costo es requerido.")
                .NotNull()
                    .WithMessage("El area al que esta asociada el centro de costo es requerido.");
        }
    }
}