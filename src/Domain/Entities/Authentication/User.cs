using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Authentication
{
    //Entidad Usuario ✅
    public class User : BaseEntity<Guid>
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? Fullname { get; set; }
        public string? PasswordHash { get; set; }
        public string? IdentificationNumber { get; set; }

        public UserType UserType { get; set; }
        public UserStatus UserStatus { get; set; }

        public virtual ICollection<Session> Sessions { get; set; } = [];
        public virtual ICollection<UserProfile> Profiles { get; set; } = [];
    }
}
