using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using System.Text.Json.Serialization;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Commands
{
    public class ProcessVacationRequestCommand : BaseRequest, IRequest<bool>
    {
        public bool IsApproved { get; set; }

        [JsonIgnore]
        public Guid VacationRequestId { get; set; }
    }
}