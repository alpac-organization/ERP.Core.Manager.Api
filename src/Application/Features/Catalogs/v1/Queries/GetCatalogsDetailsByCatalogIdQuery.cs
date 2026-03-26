using MediatR;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries
{
    public class GetCatalogsDetailsByCatalogIdQuery : IRequest<List<CatalogDetailsDto>>
    {
        public Guid CompanyId { get; set; }
        public CatalogType CatalogType { get; set; }
    }
}
