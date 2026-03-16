using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Modules.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Modules.v1.Validators
{
    /// <summary>
    /// Validador para la consulta de módulos activos por ID de empresa.
    /// </summary>
    public class CreateModuleAssociatedWithCompanyValidator : AbstractValidator<CreateModuleAssociatedWithCompanyCommand>
    {
        public CreateModuleAssociatedWithCompanyValidator()
        {
            RuleFor(x => x.ModuleName)
                .NotEmpty()
                    .WithMessage("El nombre del modulo no puede ser vacio.")
                .NotNull()
                    .WithMessage("El nombre del modulo es requerido");

        RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");
        }
    }
}