using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Absences.v1.Dtos
{
    public class AbsenceDetailDto : AbsenceDto
    {
        public string CollaboratorCode { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public Guid RegisteredBy { get; set; }
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? CancellationReason { get; set; }
        public decimal SalaryDeduction { get; set; }
        public decimal SeventhDayDeduction { get; set; }
        public decimal TravelExpensesDeduction { get; set; }
        public decimal TotalDeduction { get; set; }
        public Guid? PayrollId { get; set; }
        public Guid? OrdinaryPayrollId { get; set; }
        public Guid? DisciplinaryFaultId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
