using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Commands
{
    public class RegisterJobPositionCommand : BaseRequest, IRequest
    {
        public string? JobPositionName { get; set; }
        public string? Description { get; set; }
    }
}