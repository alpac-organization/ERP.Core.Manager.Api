using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities
{
    public class Companies : BaseEntity<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string CompanieName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public virtual ICollection<Modules> Modules { get; set; } = new List<Modules>();
    }
}