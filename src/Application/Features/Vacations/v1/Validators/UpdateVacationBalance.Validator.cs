using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Validators
{
    public class UpdateVacationBalanceValidator: AbstractValidator<UpdateVacationBalanceCommand>
    {
        public UpdateVacationBalanceValidator()
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
                    .WithMessage("El id del colaborador es requerido")
                .NotNull()
                    .WithMessage("El id del colaborador es requerido"); 
            
            RuleFor(x => x.VacationId)
                .NotEmpty()
                    .WithMessage("El id del identificador de vacaciones es requerido")
                .NotNull()
                    .WithMessage("El id del identificador de vacaciones es requerido");

            RuleFor(x => x.VacationBalance)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El saldo de vacaciones no puede ser negativo.");

            RuleFor(x => x.VacationBalance)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El saldo no puede tener más de 2 decimales.");
        }
    }
}