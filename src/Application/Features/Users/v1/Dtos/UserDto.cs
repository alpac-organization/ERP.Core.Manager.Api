namespace ERP.Core.Manager.Api.Application.Features.Users.v1.Dtos
{
    public class UserDto
    {
        string? UserId { get; set; }
        string? UserName { get; set; }
        string? Email { get; set; }

        CompanyInformation? CompanyInformation { get; set; }
    }

    public class CompanyInformation
    {
        int CompanyId { get; set; }
        string? CompanyCode { get; set; }
        string? Alias { get; set; }
        string? ImageUrl { get; set; }
    }
}