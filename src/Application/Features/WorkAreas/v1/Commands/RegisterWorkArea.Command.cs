using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Commands
{
    public class RegisterWorkAreaCommand : BaseRequest, IRequest
    {
        public string? WorkAreaName { get; set; }
        public string? Description { get; set; }
    }
}