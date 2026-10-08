using System.Text.Json.Serialization;
using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

public class GetProductDetailsQuery : BaseRequest, IRequest<ProductDetailDto>
{
    [JsonIgnore]
    public Guid ProductId { get; set; }

    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;

    public string? CommercialName { get; set; }
    public string? IdentificationNumber { get; set; }
    public SupplierExclusiveStatus? ExclusiveStatus { get; set; }
}
