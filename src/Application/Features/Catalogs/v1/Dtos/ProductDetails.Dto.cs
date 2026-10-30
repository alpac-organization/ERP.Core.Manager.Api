using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos
{
    public class ProductDetailsDto : ProductDto
    {
        public PagedResponse<ProductSupplierDto> Suppliers { get; set; } = new([], 1, 10, 0);
    }
}