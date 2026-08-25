using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

public class CreateCategoryCommand : BaseRequest, IRequest<Guid>
{
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? ParentId { get; set; }
}