namespace ERP.Core.Manager.Api.Application.Features.Reports.v1.Dtos
{
    public class ReportsDto
    {
        public List<PaymentTravelExpensesHistory> PaymentTravelExpenses { get; set; } = [];
        public List<AccumulatedHistory> AccumulatedHistory { get; set; } = [];
        public List<VacationAccrualsHistory> VacationAccrualsHistory { get; set; } = [];
        public List<InssInformation> InssInformation { get; set; } = [];
        public List<IrAndSalaryEarnedReport> IrAndSalaryEarned { get; set; } = [];
    }

    public class VacationAccrualsHistory
    {
        public DateOnly EntryDate { get; set; }
        public string? CollaboratorCode { get; set; }
        public string? CollaboratorFullname { get; set; }

        public decimal FinalBalance { get; set; }
        public decimal BeginningBalance { get; set; }

        public decimal VacationBalance { get; set; }
        public decimal EquivalesQuantity { get; set; }
        public decimal EquivalesQuantityInDollars { get; set; }
    }

    public class AccumulatedHistory
    {
        public Guid PayrollId { get; set; }
        public Guid CollaboratorId { get; set; }

        public decimal SalaryEarned { get; set; }

        public decimal AccumulatedIR { get; set; }

        public string? CollaboratorCode { get; set; }
        public string? CollaboratorFullname { get; set; }
    }

    public class PaymentTravelExpensesHistory
    {
        public Guid PayrollId { get; set; }
        public Guid CollaboratorId { get; set; }

        public string? CollaboratorCode { get; set; }
        public string? CollaboratorFullname { get; set; }

        public decimal Transport { get; set; }
        public decimal Feeding { get; set; }
        public decimal Lodging { get; set; }
    }
    public class InssInformation
    {
        public string? CollaboratorCode { get; set; }
        public string? CollaboratorFullname { get; set; }
        public decimal Income { get; set; }
        public decimal Absences { get; set; }
        public decimal InssLab { get; set; }
        public decimal InssPatronal { get; set; }

        public decimal Inatec { get; set; }
        public decimal Total { get; set; }
    }
    public class IrAndSalaryEarnedReport
    {
        public Guid PayrollId { get; set; }
        public Guid CollaboratorId { get; set; }
        public string? CollaboratorCode { get; set; }
        public string? CollaboratorFullname { get; set; }
        public decimal IrFortnightly { get; set; }
        public decimal SalaryEarnedFortnightly { get; set; }
        public decimal? IrMonthly { get; set; }
        public decimal? SalaryEarnedMonthly { get; set; }
    }

}