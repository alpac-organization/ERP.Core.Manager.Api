using MediatR;
using System.Text.Json.Serialization;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands
{
    public class ProcessPermitApplicationCommand : BaseRequest, IRequest<bool>
    {
        public bool IsApproved { get; set; }

        [JsonIgnore]
        public Guid VacationRequestId { get; set; }
    }
}