using System.Text.Json.Serialization;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Commands
{
    public class DeleteJobPositionCommand : BaseRequest, IRequest<Unit>
    {
        [JsonIgnore]
        public Guid JobPositionId { get; set; }
    }
}