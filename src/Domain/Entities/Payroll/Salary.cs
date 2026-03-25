using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Payroll
{
    public class Salary : BaseEntity<Guid>
    {
        public Guid CollaboratorId { get; set; }

        public decimal AmountInLocal { get; set; }
        public decimal AmountInForeign { get; set; }
        public decimal AmountSalary { get; set; }

        public Currency Currency { get; set; }
        public SalaryType SalaryType { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public virtual Collaborator Collaborator { get; set; } = null!;
    }
}   