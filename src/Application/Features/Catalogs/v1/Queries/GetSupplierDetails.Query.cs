using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries
{
    public class GetSupplierDetailsQuery: BaseRequest, IRequest<PagedResponse<SupplierDto>>
    {
        public Guid SupplierId { get; set; }
    }
}