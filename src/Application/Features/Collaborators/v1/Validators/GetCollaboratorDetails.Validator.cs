using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Validators
{
    public class GetCollaboratorDetailsValidator : AbstractValidator<GetCollaboratorDetailsQuery>
    {
        public GetCollaboratorDetailsValidator()
        {

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El id de usario es requerido.")
                .NotNull().WithMessage("El id de usuario es requerido.");
        }
    }

}