using MediatR;
using ERP.Core.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Commands
{
    public class RegisterWorkAreaCommand : BaseRequest, IRequest<Unit>
    {
        public bool IsOperational { get; set; } = false;
        public string? Description { get; set; }
        public string? WorkAreaName { get; set; }
    }
}