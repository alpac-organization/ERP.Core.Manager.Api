namespace ERP.Core.Manager.Api.Application.Features.Users.v1.Dtos
{
    public class UserModuleDto
    {
        public string? ModuleCode { get; set; }
        public string? ModuleName { get; set; }
        public string? RoleType { get; set; }
        public string? PathRedirect { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }
}