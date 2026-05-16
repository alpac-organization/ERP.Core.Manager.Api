namespace ERP.Core.Manager.Api.Domain.Entities.Bases
{
    public class BaseDocumentData
    {
        public string? CompanyImageUrl { get; set; }
        public string? CompanyName { get; set; }
        
        public string? CurrentDay { get; set; }
        public string? CurrentYear { get; set; }
        public string? CurrentMonth { get; set; }
        public string? CurrentMonthName { get; set; }
    }
}