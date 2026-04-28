using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Incomes.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Incomes.v1.Validators
{
    public class RegisterIncomeValidator: AbstractValidator<RegisterIncomeCommand>
    {
        public RegisterIncomeValidator()
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

            RuleFor(x => x.IdentificationNumber)
                .NotEmpty()
                    .WithMessage("La cedula del colaborador es obligatoria")
                .NotNull()
                    .WithMessage("El cedula del colaborador es obligatoria");

            RuleFor(x => x.TypeIncomeId)
                .NotEmpty()
                    .WithMessage("El tipo de ingreso es obligatorio")
                .NotNull()
                    .WithMessage("El tipo de ingreso es obligatorio");

            RuleFor(x => x.IncomeAmount)
                .NotEmpty()
                    .WithMessage("El monto del ingreso es obligatorio")
                .GreaterThanOrEqualTo(50)
                    .WithMessage("El monto del ingreso debe ser al menos 50");
        }
    }
}