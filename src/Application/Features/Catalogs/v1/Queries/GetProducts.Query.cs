using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries
{

    public class GetProductsQuery : BaseRequest, IRequest<List<ProductDto>>
    {
        public Guid? ProductId { get; set; }
    }
}