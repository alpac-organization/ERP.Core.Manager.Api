namespace ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos
{
    public class CreateUserDto
    {
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Description { get; set; }
    }
}