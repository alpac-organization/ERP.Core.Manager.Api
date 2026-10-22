using MediatR;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Database.Domain.Enums;

using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries
{
    public class GetProductsQuery : BaseRequest, IRequest<PagedResponse<ProductDto>>
    {
        public Guid? BranchId { get; set; }
        public Guid? CategoryProductId { get; set; }
        public ProductUsageType ProductUsageType { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}