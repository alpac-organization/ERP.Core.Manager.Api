using MediatR;
using ERP.Core.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

public class RegisterReasonsInventoryRemovalCommand : BaseRequest, IRequest<Unit>
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
}
