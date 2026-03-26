using ERP.Core.Manager.Api.Domain.Commons;
using ERP.Core.Manager.Api.Domain.Entities.Catalogs;

namespace ERP.Core.Manager.Api.Domain.Entities.Authentication
{
    // Perfil de usuario ✅
    public class UserProfile : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }
        public Guid CompanyId { get; set; }
        public bool IsActive { get; set; }

        public virtual User User { get; set; } = default!;        
        public virtual Company Company { get; set; } = default!;

        public virtual ICollection<UserModuleRoles> UserModuleRole { get; set; } = []; 
    }
}
