using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Validators
{
    public class GetCurrentPayrollInProgressValidator: AbstractValidator<GetCurrenPayrollInProgresssQuery>
    {
        public GetCurrentPayrollInProgressValidator()
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

            RuleFor(x => x.Type)
                .NotEmpty()
                    .WithMessage("El tipo de nomina es requerido")
                .NotNull()
                    .WithMessage("El tipo de nomina es requerido");
        }
    }
}