using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Authentication
{
    //Entidad Permiso ✅
    public class Permission : BaseEntity<Guid>
    {
        public Guid RoleId { get; set; } 
        public string? Description { get; set; }
        public string? PermissionName { get; set; }      
        public PermissionType PermissionType { get; set; }

        public virtual Role Role { get; set; } = null!;
    }
}