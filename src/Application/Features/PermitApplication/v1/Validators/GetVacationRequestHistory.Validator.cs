using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Validators
{
    public class GetVacationRequestHistoryValidator : AbstractValidator<GetPermitApplicationHistoryQuery>
    {
        public GetVacationRequestHistoryValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.ModuleCode)
                .NotEmpty()
                    .WithMessage("El codigo de modulo es requerido")
                .NotNull()
                    .WithMessage("El codigo de modulo es requerido");

            RuleFor(x => x.UserId)
                .NotEmpty()
                    .WithMessage("El id de usuario es requerido")
                .NotNull()
                    .WithMessage("El id de usuario es requerido");
            
            RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("El número de página debe ser mayor que 0.");
            
            RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("El tamaño de página debe ser mayor que 0.");
        }
    }
}