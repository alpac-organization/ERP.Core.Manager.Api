using ERP.Core.Manager.Api.Domain.Commons;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Domain.Entities.Authentication
{
    public class Session : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }
        public string? Device { get; set; }
        public string? IpAddress { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        
        public DateTime ExpiresAt { get; set; }
        public virtual User User { get; set; } = null!;
    }
}