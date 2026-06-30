namespace ERP.Core.Manager.Api.Application.Features.Reports.v1.Dtos
{
    public class ReportsDto
    {
        public List<PaymentTravelExpensesHistory> PaymentTravelExpenses { get; set; } = [];
        public List<AccumulatedHistory> AccumulatedHistory { get; set; } = [];
        public List<VacationAccrualsHistory> VacationAccrualsHistory { get; set; } = [];
        public List<SubsidyHistoryDto> SubsidiesHistory { get; set; } = [];
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

    public class SubsidyHistoryDto
    {
        public string? CollaboratorCode { get; set; }
        public string? CollaboratorFullName { get; set; }
        public int AmountDays { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? TypeSubsidyName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal Percentage { get; set; }
        public decimal CompanyAssumedAmount { get; set; }
        public decimal InssReimbursementAmount { get; set; }
    }

}