using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Authentication
{
    public class UserProfile : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }
        public int CompanyId { get; set; }
        public bool IsActive { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
