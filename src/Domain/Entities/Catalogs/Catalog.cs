using ERP.Core.Manager.Api.Domain.Commons;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Domain.Entities.Catalogs
{
    public class Catalog : BaseEntity<int>
    {
        public string? CatalogName { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
        public bool IsActive { get; set; }
        public CatalogType CatalogType { get; set; }       

        public virtual Company Company { get; set; } = null!;
        public virtual ICollection<SubCatalog> SubCatalogs { get; set; } = [];       
    }
}