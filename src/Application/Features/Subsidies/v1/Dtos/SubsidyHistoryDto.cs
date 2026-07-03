namespace ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Dtos
{
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