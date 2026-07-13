using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Validators
{
    public class DeleteWorkAreaValidator: AbstractValidator<DeleteWorkAreaCommand>
    {
        public DeleteWorkAreaValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.WorkAreaId)
                .NotEmpty()
                    .WithMessage("Debes seleccionar la area")
                .NotNull()
                    .WithMessage("Debes seleccionar la area");
        }
    }
}