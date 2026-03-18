using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Users.v1.Dtos
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public UserStatus Status { get; set; }
    }
}