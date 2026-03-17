using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Authentication
{
    public class UserModuleRoles : BaseEntity<Guid>
    {
        public Guid RoleId { get; set; }
        public string? ModuleCode { get; set; }
        public Guid UserProfileId { get; set; }
        public bool IsActive { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual UserProfile UserProfile { get; set; } = null!;
    }
}
