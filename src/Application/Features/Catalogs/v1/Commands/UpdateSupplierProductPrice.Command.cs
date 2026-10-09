using System.Text.Json.Serialization;
using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

public class UpdateSupplierProductPriceCommand : BaseRequest, IRequest<bool>
{
    [JsonIgnore]
    public Guid SupplierId { get; set; }

    public Currency? Currency { get; set; }

    [JsonIgnore]
    public Guid ProductId { get; set; }

    public decimal? NewUnitPrice { get; set; }

    public List<TierPriceDto>? TierPrices { get; set; }
}
