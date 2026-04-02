namespace ERP.Core.Manager.Api.Application.Features.Companies.v1.Dtos
{
    public class CompanyDto
    {
        public Guid CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? ImageUrl { get; set; }
        public string? NeutralImageUrl { get; set; }
        public string? Alias { get; set; }
    }
}