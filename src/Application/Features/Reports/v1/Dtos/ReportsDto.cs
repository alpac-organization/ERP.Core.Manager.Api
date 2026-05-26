namespace ERP.Core.Manager.Api.Application.Features.Reports.v1.Dtos
{
    public class ReportsDto
    {
        public List<PaymentTravelExpensesHistory> PaymentTravelExpenses { get; set; } = [];
        public List<AccumulatedHistory> AccumulatedHistory { get; set; } = [];
        public List<VacationAccrualsHistory> VacationAccrualsHistory { get; set; } = [];
    }

    public class VacationAccrualsHistory
    {
        public string? CollaboratorCode { get; set; }
        public string? CollaboratorFullname { get; set; }
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

    public class  PaymentTravelExpensesHistory
    {
        public Guid PayrollId { get; set; }
        public Guid CollaboratorId { get; set; }

        public string? CollaboratorCode { get; set; }
        public string? CollaboratorFullname { get; set; }

        public decimal Transport { get; set; }
        public decimal Feeding { get; set; }
        public decimal Lodging { get; set; }
    }

}