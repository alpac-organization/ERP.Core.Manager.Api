using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Branches.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Branches.v1.Validators
{
    public class RegisterBranchValidator : AbstractValidator<RegisterBranchCommand>
    {
        public RegisterBranchValidator()
        {

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El id de usario es requerido.")
                .NotNull().WithMessage("El id de usuario es requerido.");

            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("El id de la empresa es requerido.")
                .NotNull().WithMessage("El id de la empresa es requerido.");

            RuleFor(x => x.BrachName)
                .NotEmpty().WithMessage("El nombre de la empresa es requerido")
                .NotNull().WithMessage("El nombre de la empresa es requerido");
        }
    }
}