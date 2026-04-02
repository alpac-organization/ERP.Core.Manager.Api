using ERP.Core.Manager.Api.Domain.Commons;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Domain.Entities.Payroll
{
    public class VacationRequest : BaseEntity<Guid>
    {
        public Guid CollaboratorId { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string?  ApprovedBy { get; set; }
        public VacationRequestStatus Status { get; set; }

        public string? RequestedBy { get; set; }
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public virtual Collaborator Collaborator { get; set; } = null!;
    }
}