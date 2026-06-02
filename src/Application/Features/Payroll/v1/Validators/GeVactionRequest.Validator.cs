using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries;
using FluentValidation;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Validators
{
    public class GetVacationRequestValidator: AbstractValidator<GetPermitApplicationInPayrollQuery>
    {
        public GetVacationRequestValidator()
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

            RuleFor(x => x.BranchId)
                .NotEmpty()
                    .WithMessage("Seleccione una sucursal")
                .NotNull()
                    .WithMessage("Seleccione una sucursal");
        }
    }
}