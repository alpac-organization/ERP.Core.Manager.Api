using ERP.Core.Manager.Api.Domain.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Catalogs;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories
{
    public interface ISubCatalogsRepository : IRepository<SubCatalog>
    {
        Task<List<SubCatalog>> GetSubCatalogsByCatalogId(int CatalogId, CancellationToken cancellationToken);
    }
}