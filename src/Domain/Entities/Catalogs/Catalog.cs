using ERP.Core.Manager.Api.Domain.Commons;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Domain.Entities.Catalogs
{
    public class Catalog : BaseEntity<int>
    {
        public bool IsActive { get; set; }
        public string? CatalogName { get; set; }
        public string? Description { get; set; }
        public bool? IsGlobal { get; set; }
        public CatalogType CatalogType { get; set; }       

        public Guid? CompanyId { get; set; }
        public virtual Company Company { get; set; } = null!;

        public virtual ICollection<SubCatalog> SubCatalogs { get; set; } = [];       
    }
}