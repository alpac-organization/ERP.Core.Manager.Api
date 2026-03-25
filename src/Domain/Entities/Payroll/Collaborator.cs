using ERP.Core.Manager.Api.Domain.Commons;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Domain.Entities.Payroll
{
    public class Collaborator : BaseEntity<Guid>
    {
        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public string? ThirdName { get; set; }
        public string? FirstLastname { get; set; }
        public string? SecondLastname { get; set; }
        public string? IdentificationNumber { get; set; }

        public GenderType Gender { get; set; }
        public CollaboratorStatus Status { get; set; }
        public IdentificationType IdentificationType { get; set; }

        public virtual ICollection<VacationRequest> VacationRequests { get; set; } = [];      
    }
}