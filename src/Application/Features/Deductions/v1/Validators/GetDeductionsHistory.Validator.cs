using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Validators
{
    public class GetDeductionsHistoryValidator: AbstractValidator<GetDeductionsHistoryQuery>
    {
        public GetDeductionsHistoryValidator()
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
        }
    }
}