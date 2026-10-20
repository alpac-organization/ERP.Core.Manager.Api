using MediatR;
using System.Text.Json.Serialization;
using ERP.Core.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

public class DeactivateSupplierProductCommand : BaseRequest, IRequest<bool>
{
    [JsonIgnore]
    public Guid SupplierId { get; set; }

    [JsonIgnore]
    public Guid ProductId { get; set; }
}
