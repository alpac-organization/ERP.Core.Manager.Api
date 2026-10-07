using System.Text.Json.Serialization;
using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

public class UpdateSupplierExclusiveStatusCommand : BaseRequest, IRequest<bool>
{
    [JsonIgnore]
    public Guid SupplierId { get; set; }

    public SupplierExclusiveStatus ExclusiveStatus { get; set; }

    public string? Comments { get; set; }
}
