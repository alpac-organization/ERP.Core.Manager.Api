namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos
{
    public class CatalogDetailsDto
    {
        public int SubCatalogId { get; set; }
        public string? CatalogName { get; set; }
        public string? Description { get; set; }
        public int CatalogId { get; set; }
    }
}
