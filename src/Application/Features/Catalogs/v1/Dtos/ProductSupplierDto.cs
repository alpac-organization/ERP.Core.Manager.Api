using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

public class ProductSupplierDto
{
    public Guid SupplierProductId { get; set; }
    public Guid SupplierId { get; set; }
    public string? SupplierLegalName { get; set; }
    public string? CommercialName { get; set; }
    public SupplierType SupplierType { get; set; }
    public Currency Currency { get; set; }
    public Guid UnitMeasureId { get; set; }
    public decimal UnitPrice { get; set; }
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
    public Currency Currency { get; set; }
}

public class SupplierProductPriceHistoryDto
{
    public Guid HistoryPriceId { get; set; }
    public SupplierPriceHistoryType PriceType { get; set; }
    public decimal Price { get; set; }
    public int? MinQuantity { get; set; }
    public Currency Currency { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsCurrent { get; set; }
    public PriceWindowStatus Status { get; set; }
}

public enum PriceWindowStatus
{
    Current = 1,
    Scheduled = 2,
    Historical = 3
}