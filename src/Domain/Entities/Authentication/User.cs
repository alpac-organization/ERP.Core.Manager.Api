using ERP.Core.Manager.Api.Domain.Commons;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Domain.Entities.Authentication
{
    //Entidad Usuario ✅
    public class User : BaseEntity<Guid>
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? PasswordHash { get; set; }
        public UserStatus UserStatus { get; set; }
        public string? Fullname { get; set; }

        public virtual ICollection<Session> Sessions { get; set; } = [];
        public virtual ICollection<UserProfile> Profiles { get; set; } = [];

    }
}
