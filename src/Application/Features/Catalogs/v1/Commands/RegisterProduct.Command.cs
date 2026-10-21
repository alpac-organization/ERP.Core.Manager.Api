using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

public class RegisterProductCommand : BaseRequest, IRequest<Guid>
{
    public string? Description { get; set; }
    public string ProductName { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public Guid UnitMeasureId { get; set; }

    public bool IsTaxExempt { get; set; }
    public ProductUsageType ProductUsageType { get; set; }
    public List<ProductSupplierItemDto> Suppliers { get; set; } = [];

    public ConfigurationStockToInventory? ConfigurationStock { get; set; }
}

public class ConfigurationStockToInventory
{
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
}

public class ProductSupplierItemDto
{
    public Guid SupplierId { get; set; }
    public decimal UnitPrice { get; set; }
    public Guid? UnitMeasureId { get; set; }

    public Currency? Currency { get; set; }

    public List<TierPriceDto>? TierPrices { get; set; }
}

public class TierPriceDto
{
    public int? MinQuantity { get; set; }
    public decimal? PreferentialPrice { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public Guid? UnitMeasureId { get; set; }
    public Currency? Currency { get; set; }
}
