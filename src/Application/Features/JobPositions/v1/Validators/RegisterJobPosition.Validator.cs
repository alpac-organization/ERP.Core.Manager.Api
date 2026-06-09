using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Validators
{
    public class RegisterJobPositionValidator: AbstractValidator<RegisterJobPositionCommand>
    {
        public RegisterJobPositionValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.JobPositionName)
                .NotEmpty()
                    .WithMessage("Asegurese de ingresar el nombre del cargo es obligatorio.")
                .NotNull()
                    .WithMessage("Asegurese de ingresar el nombre del cargo es obligatorio.");

        }
    }
}