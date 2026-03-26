using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Catalogs
{
    public class Module: BaseEntity<Guid>
    {
        public bool IsActive { get; set; }    
        public Guid CompanyId { get; set; }

        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? ModuleName { get; set; }

        public virtual Company Company { get; set; } = default!;
    }
}