using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Commands
{
    public class DeleteJobPositionCommand : BaseRequest, IRequest<bool>
    {
        public Guid JobPositionId { get; set; }
    }
}