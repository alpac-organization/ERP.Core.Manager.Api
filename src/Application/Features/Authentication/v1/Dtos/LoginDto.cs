namespace ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos
{
    public class LoginDto
    {
        public string? UserName { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public CompanyInformation CompanyInformation { get; set; } = new();
    }

    public class CompanyInformation
    {
        public int CompanyId { get; set; }
        public string? ImageUrl { get; set; }
        public string? CompanyName { get; set; }
    }
}