using ERP.Core.Manager.Api.Domain.Commons;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Catalogs;

namespace ERP.Core.Manager.Api.Domain.Entities.Payroll
{
    public class Collaborator : BaseEntity<Guid>
    {
        required public string FirstName { get; set; }
        required public string FirstLastname { get; set; }
        required public string IdentificationNumber { get; set; }
        required public string CollaboratorCode { get; set; }

        //Id de la empresa a la que pertenece este colaborador.
        required public int CompanyId { get; set; }

        //Otras propiedades
        public string? SecondName { get; set; }
        public string? ThirdName { get; set; }
        public string? SecondLastname { get; set; }
        public string? RegisteredBy { get; set; }

        public GenderType Gender { get; set; }
        public CollaboratorStatus Status { get; set; }
        public IdentificationType IdentificationType { get; set; }

        // Relacionar tablas para acceso a  ellas
        public virtual Company Company { get; set; } = default!;
        public virtual Vacation Vacation { get; set; } = default!;
        public virtual PersonalInformation PersonalInformation { get; set; } = default!;
        public virtual WorkingInformation WorkingInformation { get; set; } = default!;

        //Multiples datos
        public virtual ICollection<Salary> Salaries { get; set; } = [];
        public virtual ICollection<VacationRequest> VacationRequests { get; set; } = [];  
    }
}