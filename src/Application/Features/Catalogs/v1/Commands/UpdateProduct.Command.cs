using System.Text.Json.Serialization;
using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

public class UpdateProductCommand : BaseRequest, IRequest<bool>
{
    [JsonIgnore]
    public Guid ProductId { get; set; }

    public string? ProductName { get; set; }
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? UnitMeasureId { get; set; }
    public ProductUsageType? ProductUsageType { get; set; }
    public bool? IsTaxExempt { get; set; }
}
