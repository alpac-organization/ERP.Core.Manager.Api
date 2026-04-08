using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Payroll
{
    public class PermitApplication : BaseEntity<Guid>
    {
        public Guid CollaboratorId { get; set; }

        public string?  ApprovedBy { get; set; }
        public string?  RejectedBy { get; set; }
        public string? CollaboratorCode { get; set; }

        public PermitApplicationType Type { get; set; }
        public PermitApplicationStatus Status { get; set; }

        public int AmountDays { get; set; }

        public string? RequestedBy { get; set; }
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        public virtual Collaborator Collaborator { get; set; } = null!;
    }
}