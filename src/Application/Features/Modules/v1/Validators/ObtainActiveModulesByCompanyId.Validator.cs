using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Modules.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Modules.v1.Validators
{
    /// <summary>
    /// Validador para la consulta de módulos activos por ID de empresa.
    /// </summary>
    public class ObtainActiveModulesByCompanyIdValidator : AbstractValidator<ObtainActiveModulesByCompanyIdQuery>
    {
        public ObtainActiveModulesByCompanyIdValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El ID de la empresa es obligatorio.");
        }
    }
}