namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos
{
    public class ProductDto
    {
        public Guid ProductId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string ProductName { get; set; } = null!;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public Guid UnitMeasureId { get; set; }
        public ProductCategoryDto Category { get; set; } = default!;
        public int SuppliersCount { get; set; }
    }

    public class ProductCategoryDto
    {
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public bool IsActive { get; set; }
    }
}
