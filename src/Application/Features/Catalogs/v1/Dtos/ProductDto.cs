using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Shopping.v1.Dtos;
namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos
{
    public class ProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public ProductUsageType UsageType { get; set; }

        public CategoryProductDto Category { get; set; } = default!;
        public List<QuoteDetailDto> QuoteDetails { get; set; } = [];
    }
}