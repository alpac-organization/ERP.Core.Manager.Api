using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

public class RegisterProductCommand : BaseRequest, IRequest<Guid>
{
    public string ProductName { get; set; } = null!;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public Guid UnitMeasureId { get; set; }

    public ProductUsageType ProductUsageType { get; set; }
    public bool IsTaxExempt { get; set; }

    public List<ProductSupplierItemDto> Suppliers { get; set; } = [];
}

public class ProductSupplierItemDto
{
    public Guid SupplierId { get; set; }
    public decimal UnitPrice { get; set; }
    
    public List<TierPriceDto>? TierPrices { get; set; } = [];
}

public class TierPriceDto
{
    public int MinQuantity { get; set; }
    public decimal PreferentialPrice { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public Guid? UnitMeasureId { get; set; }
}
