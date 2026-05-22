using System.Text.Json.Serialization;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Commands
{
    public class RegisterSubsidyCommmand: BaseRequest, IRequest<bool>
    {
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }

        
        public Guid PayrollId { get; set; }
        public Guid TypeSubsidyId { get; set; }

        [JsonIgnore]
        public Guid CollaboratorId { get; set; }

        public decimal Percentage { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Observations { get; set; }
    }
}