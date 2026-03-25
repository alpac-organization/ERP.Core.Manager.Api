using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Payroll
{
    public class WorkingInformation : BaseEntity<Guid>
    {
        public Guid CollaboratorId { get; set; }
        public int WorkAreaId { get; set; }
        public int WorkPositionId { get; set; }
        public string? WorkPhonNumber { get; set; }
        public string? WorkEmail { get; set; }

        public virtual Collaborator Collaborator { get; set; } = null!;
    }
}