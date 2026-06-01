using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Validators
{
    public class GetWorkAreaValidator: AbstractValidator<GetWorkAreasQuery>
    {
        public GetWorkAreaValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");
        }
    }
}