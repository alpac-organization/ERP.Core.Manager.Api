using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Authentication
{
    // Entidad Role✅
    public class Role : BaseEntity<Guid>
    {
        public RoleType RoleType { get; set; }
        public string? RoleName { get; set; }
        public string? Description { get; set; }
        
        public virtual ICollection<Permission> Permissions { get; set; } = [];
        public virtual ICollection<UserModuleRoles> UserModuleRoles { get; set; } = [];
    }
}