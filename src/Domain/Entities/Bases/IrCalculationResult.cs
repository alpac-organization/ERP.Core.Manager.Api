namespace ERP.Core.Manager.Api.Domain.Entities.Bases
{
    public record IrCalculationResult(
        decimal BiweeklyInss, 
        decimal BiweeklyIr
    );
}