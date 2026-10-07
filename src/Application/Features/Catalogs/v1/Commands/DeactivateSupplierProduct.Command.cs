using System.Text.Json.Serialization;
using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

public class DeactivateSupplierProductCommand : BaseRequest, IRequest<bool>
{
    [JsonIgnore]
    public Guid SupplierId { get; set; }

    [JsonIgnore]
    public Guid ProductId { get; set; }
}
