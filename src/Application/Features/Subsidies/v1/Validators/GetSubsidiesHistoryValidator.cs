using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Validators
{
    public class GetSubsidiesHistoryValidator : AbstractValidator<GetSubsidiesHistoryQuery>
    {
        public GetSubsidiesHistoryValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("El id de la empresa es requerido.");

            RuleFor(x => x.ModuleCode)
                .NotEmpty()
                .WithMessage("El código de módulo es requerido.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("El id de usuario es requerido.");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("El número de página debe ser mayor que 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("El tamaño de página debe ser mayor que 0.");
        }
    }
}