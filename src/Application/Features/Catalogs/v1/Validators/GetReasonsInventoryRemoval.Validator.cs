using FluentValidation;
using ERP.Core.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

public class GetReasonsInventoryRemovalValidator : BaseRequestValidator<GetReasonsInventoryRemovalQuery>
{
    public GetReasonsInventoryRemovalValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("El número de página debe ser mayor que 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("El tamaño de página debe ser mayor que 0.");

    }
}