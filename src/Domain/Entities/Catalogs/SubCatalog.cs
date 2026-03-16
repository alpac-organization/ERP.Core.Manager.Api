using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Catalogs
{
    public class SubCatalog : BaseEntity<int>
    {
        public string? CatalogName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int CatalogId { get; set; }
        public virtual Catalog Catalog { get; set; } = null!;
    }
}