using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

public class RegisterProductCommand : BaseRequest, IRequest<bool>
{
    public string ProductName { get; set; } = null!;
    public string? Description { get; set; }
    public ProductUsageType UsageType { get; set; }
    public Guid CategoryId { get; set; }
}