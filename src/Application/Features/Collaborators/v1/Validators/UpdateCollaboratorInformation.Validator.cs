using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Validators
{
    public class UpdateCollaboratorInformationValidator : AbstractValidator<UpdateCollaboratorInformationCommand>
    {
        public UpdateCollaboratorInformationValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El id de usario es requerido.")
                .NotNull().WithMessage("El id de usuario es requerido.");

            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("El id de la empresa es requerido")
                .NotNull().WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.ModuleCode)
                .NotEmpty().WithMessage("El codigo de modulo es requerido")
                .NotNull().WithMessage("El codigo de modulo es requerido");

            RuleFor(x => x.IdentificationNumber)
                .NotEmpty().WithMessage("La identificación del colaborador es requerida")
                .NotNull().WithMessage("La identificación del colaborador es requerida");
        }
    }

}