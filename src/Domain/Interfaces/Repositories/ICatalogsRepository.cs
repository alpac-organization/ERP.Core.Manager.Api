using ERP.Core.Manager.Api.Domain.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities;
using ERP.Core.Manager.Api.Domain.Entities.Catalogs;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories
{
    public interface ICatalogsRepository : IRepository<Catalog>
    {
        // Task<List<TEntity>> ObtenerCatalogDispoibles(T catalogType);
    }
}