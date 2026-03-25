using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Payroll
{
    public class Vacation : BaseEntity<Guid>
    {
        public Guid CollaboratorId { get; set; }
        public float AvailableVacations { get; set; }
        public float GeneredVacation { get; set; }
        public float EnjoyedVacation { get; set; }
        
        public virtual Collaborator Collaborator { get; set; } = null!;
    }
}