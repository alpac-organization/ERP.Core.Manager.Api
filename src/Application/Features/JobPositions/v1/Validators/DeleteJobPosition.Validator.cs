using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Validators
{
    public class DeleteJobPositionValidator: AbstractValidator<DeleteJobPositionCommand>
    {
        public DeleteJobPositionValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.JobPositionId)
                .NotEmpty()
                    .WithMessage("El id del cargo es requerido.")
                .NotNull()
                    .WithMessage("El id del cargo es requerido.");
        }
    }
}