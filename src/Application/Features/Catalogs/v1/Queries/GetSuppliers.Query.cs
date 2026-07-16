using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries
{
    public class GetSuppliersQuery: BaseRequest, IRequest<PagedResponse<SupplierDto>>
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}