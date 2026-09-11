using MediatR;
using System.Text.Json.Serialization;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Commands
{
    public class DeleteWorkAreaCommand : BaseRequest, IRequest<Unit>
    {
        [JsonIgnore]
        public Guid WorkAreaId { get; set; }
    }
}
