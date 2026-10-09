using MediatR;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries
{
    public class GetPurchaseRejectionReasonsQuery : IRequest<List<CatalogDetailsDto>>
    {
        public Guid CompanyId { get; set; }
    }
}
