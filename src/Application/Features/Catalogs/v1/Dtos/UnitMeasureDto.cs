using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos
{
    public class UnitMeasureDto
    {
        public Guid UnitMeasureId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Symbol { get; set; } = default!;
        public string? Description { get; set; }

        public UnitMeasureType Type { get; set; }
    }
}