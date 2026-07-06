namespace ERP.Core.Manager.Api.Domain.Entities.Bases
{
    public record IndemnificationResult(
        decimal YearsOfService,
        decimal IndemnificationValue
    );
}