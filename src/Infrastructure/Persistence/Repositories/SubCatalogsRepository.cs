using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories
{
    public class SubCatalogsRepository(AppDbContext _context) : Repository<SubCatalog>(_context), ISubCatalogsRepository
    {
        public async Task<List<SubCatalog>> GetSubCatalogsByCatalogId(int CatalogId, CancellationToken cancellationToken)
        {
            return await _context.SubCatalogs
                .Where(sc => sc.CatalogId == CatalogId && sc.IsActive)
                .ToListAsync(cancellationToken);
        }
    }
}