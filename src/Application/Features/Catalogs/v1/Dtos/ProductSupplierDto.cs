using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

public class ProductSupplierDto
{
    public Guid SupplierProductId { get; set; }
    public Guid SupplierId { get; set; }
    public string? SupplierLegalName { get; set; }
    public string? CommercialName { get; set; }
    public decimal UnitPrice { get; set; }
    public Currency Currency { get; set; }
    public DateTime LastPriceUpdate { get; set; }
    public bool IsActive { get; set; }
    public List<TierPriceResponseDto> TierPrices { get; set; } = [];
}
public class TierPriceResponseDto
{
    public Guid TierPriceId { get; set; }
    public int MinQuantity { get; set; }
    public decimal PreferentialPrice { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public Guid? UnitMeasureId { get; set; }
}

public class SupplierProductPriceHistoryDto
{
    public Guid HistoryPriceId { get; set; }
    public SupplierPriceHistoryType PriceType { get; set; }
    public decimal Price { get; set; }
    public int? MinQuantity { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }
    public bool IsCurrent => EffectiveTo == DateTime.MaxValue;
}

public class ProductDetailDto
{
    public Guid ProductId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string ProductName { get; set; } = null!;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public ProductCategoryDto Category { get; set; } = default!;
    public Guid UnitMeasureId { get; set; }
    public string? UnitMeasureName { get; set; }
    public ProductUsageType ProductUsageType { get; set; }
    public bool IsTaxExempt { get; set; }
    public PagedResponse<ProductSupplierDto> Suppliers { get; set; } = new([], 1, 10);
}
