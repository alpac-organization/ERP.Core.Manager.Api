using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Catalogs
{
    public class Company : BaseEntity<int>
    {
        public string Code { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string CompanieName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        
        public virtual ICollection<Module> Modules { get; set; } = [];
        public virtual ICollection<Catalog> Catalogs { get; set; } = [];

    }
}