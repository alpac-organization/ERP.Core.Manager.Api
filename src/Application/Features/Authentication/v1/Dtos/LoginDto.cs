namespace ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos
{
    public class LoginDto
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? UserType { get; set; }
        public Guid BranchId { get; set; }
        public CompanyInformation CompanyInformation { get; set; } = new();
    }

    public class CompanyInformation
    {
        public Guid CompanyId { get; set; }
        public string? Alias { get; set; }
        public string? ImageUrl { get; set; }
        public string? NeutralImageUrl { get; set; }
        public string? CompanyName { get; set; }
    }
}