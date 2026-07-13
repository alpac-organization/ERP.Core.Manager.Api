using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Validators
{
    public class GetJobPositionValidator: AbstractValidator<GetJobPositionsQuery>
    {
        public GetJobPositionValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");
        }
    }
}