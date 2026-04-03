using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos
{
    public class VacationRequestDto
    {
        public Guid VacationRequestId { get; set; }
        public Guid CollaboratorId { get; set; }
        public string? CollaboratorCode { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public string? Description { get; set; }
        public string? RequestedBy { get; set; }

        public string?  ApprovedBy { get; set; }
        public string?  RejectedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public VacationRequestStatus Status { get; set; }
    }
}