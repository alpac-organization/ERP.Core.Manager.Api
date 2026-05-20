namespace ERP.Core.Manager.Api.Application.Features.Reports.v1.Dtos
{
    public class ReportsDto
    {
        public List<AccumulatedHistory> AccumulatedHistory { get; set; } = [];
        public List<VacationAccrualsHistory> VacationAccrualsHistory { get; set; } = [];
    }

    public class VacationAccrualsHistory
    {
        
    }

    public class AccumulatedHistory
    {
        public Guid PayrollId { get; set; }
        public Guid CollaboratorId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public decimal SalaryEarned { get; set; }
        public decimal AccumulatedIR { get; set; }

        public string? CollaboratorCode { get; set; }
        public string? CollaboratorFullname { get; set; }
    }

}