using ERP.Core.Manager.Api.Domain.Commons;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;

namespace ERP.Core.Manager.Api.Domain.Entities.Catalogs
{
    public class Company : BaseEntity<int>
    {
        required public string Code { get; set; } = string.Empty;
        required public string Alias { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        required public string CompanieName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        
        public virtual ICollection<Module> Modules { get; set; } = [];
        public virtual ICollection<Catalog> Catalogs { get; set; } = [];
        public virtual ICollection<Collaborator> Collaborators { get; set; } = [];
    }
}