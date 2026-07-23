using ERP.Core.Database.Domain.Enums;
namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos
{
    public class ProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public ProductUsageType UsageType { get; set; }

        public Category
    }
}
/*
"category_details": {
                    "category_id": "e067bbbd-48cf-44fa-a349-352b3a149175",
                    "parent_id": null,
                    "category_name": "Papelería Impresa",
                    "category_code": "PRINTED_STATIONARY"
                }
*/