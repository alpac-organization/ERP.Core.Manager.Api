using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Reports.v1.Queries;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Reports.v1.Validators
{
    public class GetReportsByTypeValidator : AbstractValidator<GetReportsByTypeQuery>
    {
        public GetReportsByTypeValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.UserId)
                .NotEmpty()
                    .WithMessage("El usuario es requerido")
                .NotNull()
                    .WithMessage("El usuario es requerido");

            RuleFor(x => x.Type)
                .NotEmpty()
                    .WithMessage("El tipo de reporte es requerido")
                .NotNull()
                    .WithMessage("El tipo de reporte es requerido");

            RuleFor(x => x.PayrollId)
                .NotEmpty()
                .When(x => x.Type == ReportsType.JudicialSeizures)
                .WithMessage("El id de la nómina es obligatorio para el reporte de embargos judiciales");
        }
    }
}