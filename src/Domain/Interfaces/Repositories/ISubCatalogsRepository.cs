using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Domain.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories
{
    public interface ISubCatalogsRepository : IRepository<SubCatalog>
    {
        Task<List<SubCatalog>> GetSubCatalogsByCatalogId(int CatalogId, CancellationToken cancellationToken);
    }
}