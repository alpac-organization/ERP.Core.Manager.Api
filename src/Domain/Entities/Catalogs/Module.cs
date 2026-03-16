using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Catalogs
{
    public class Module: BaseEntity<int>
    {
        public bool IsActive { get; set; } = true;
        public string ModuleName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; } = null!;
    }
}