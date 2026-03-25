using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Payroll
{
    public class PersonalInformation : BaseEntity<Guid>
    {

        public Guid CollaboratorId { get; set; }
        public string? PersonalEmail { get; set; }
        public string? PersonalPhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Departament { get; set; }

        public virtual Collaborator Collaborator { get; set; } = null!;
    }
}