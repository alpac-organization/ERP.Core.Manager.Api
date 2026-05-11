using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Commands;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Validators
{
    public class RegisterDeductionValidator: AbstractValidator<RegisterDeductionCommand>
    {
        public RegisterDeductionValidator()
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

            When(x => x.DeductionType == DeductionType.SalaryAdvance, () =>
            {
                RuleFor(x => x.AdvanceSalaryPayload)
                    .NotNull().WithMessage("Los datos para adelato de salario son obligatorios")
                    .SetValidator(new AdvanceSalaryPayloadValidator());
            });
        }
    }

    public class  AdvanceSalaryPayloadValidator: AbstractValidator<AdvanceSalaryPayload?>
    {
        public AdvanceSalaryPayloadValidator()
        {
            RuleFor(x => x!.Amount)
                .NotEmpty().WithMessage("La cantidad de salario a adelantar es obligatoria")
                .GreaterThan(0).WithMessage("La cantidad de salario a adelatar debe ser mayor a 0.");

            RuleFor(x => x!.Currency)
                .NotEmpty().WithMessage("La moneda es obligatoria");
        }
    }
}