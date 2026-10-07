using System.Text.Json.Serialization;
using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

public class GetSupplierProductPriceHistoryQuery : BaseRequest, IRequest<List<SupplierProductPriceHistoryDto>>
{
    [JsonIgnore]
    public Guid SupplierId { get; set; }

    [JsonIgnore]
    public Guid ProductId { get; set; }
}
