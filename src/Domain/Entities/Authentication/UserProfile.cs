using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Authentication
{
    // Perfil de usuario ✅
    public class UserProfile : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }
        public int CompanyId { get; set; }
        public bool IsActive { get; set; }

        public virtual User User { get; set; } = null!;
        
        // Modulos a los que tiene acceso, con su respectivo role
        public virtual ICollection<UserModuleRoles> UserModuleRole { get; set; } = []; 
    }
}
