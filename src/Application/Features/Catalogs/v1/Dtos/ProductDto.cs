using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Branches.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos
{
    public class ProductDto
    {
        public Guid ProductId { get; set; }
        public string? Code { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public ProductUsageType UsageType { get; set; }

        public Guid CategoryId { get; set; }
        public Guid UnitMeasureId { get; set; }
        public bool IsTaxExempt { get; set; }

        public int SuppliersCount { get; set; }

        public BranchesDto? BranchInformation { get; set; }
        public CostCenterDto? CostCenterInformation { get; set; }
        public UnitMeasureDto? UnitMeasureInformation { get; set; }
        public CategoryProductDto? Category { get; set; }
    }
}
