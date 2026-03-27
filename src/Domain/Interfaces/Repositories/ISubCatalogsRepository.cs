using ERP.Core.Manager.Api.Domain.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories
{
    public interface ISubCatalogsRepository : IRepository<SubCatalog>
    {
        Task<List<SubCatalog>> GetSubCatalogsByCatalogId(int CatalogId, CancellationToken cancellationToken);
    }
}