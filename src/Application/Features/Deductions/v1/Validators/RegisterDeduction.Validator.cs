using FluentValidation;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Commands;

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

            RuleFor(x => x.PayrollId)
                .NotEmpty()
                    .WithMessage("El id del periodo de nomina para registrar deducción es obligatorio")
                .NotNull()
                    .WithMessage("El id del periodo de nomina para registrar deducción es obligatorio");

            RuleFor(x => x.DeductionType)
                .NotEmpty()
                    .WithMessage("El tipo de deducción es obligatorio")
                .NotNull()
                    .WithMessage("El tipo de deducción es obligatorio");
        }
    }
}