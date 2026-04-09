using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Payroll
{
    public class Vacation : BaseEntity<Guid>
    {
        required public Guid CollaboratorId { get; set; }
        
        //Solicitar Pago de vacaciones donadas
        public decimal AvailableVacations { get; set; }
        public decimal GeneredVacation { get; set; }
        public decimal EnjoyedVacation { get; set; }
        public decimal DonatedVacation { get; set; }
        
        public virtual Collaborator Collaborator { get; set; } = null!;
    }
}