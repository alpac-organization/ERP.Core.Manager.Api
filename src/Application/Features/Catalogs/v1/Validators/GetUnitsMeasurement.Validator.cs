using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators
{
    public class GetUnitsMeasurementValidator : AbstractValidator<GetUnitsMeasurementQuery>
    {
        public GetUnitsMeasurementValidator()
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

            RuleFor(x => x.UnitMeasureType)
                .IsInEnum()
                .When(x => x.UnitMeasureType.HasValue);
        }
    }

}