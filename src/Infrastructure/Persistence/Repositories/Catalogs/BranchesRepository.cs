using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Catalogs;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Catalogs
{
    public class BranchesRepository(AppDbContext _context) : Repository<Branch>(_context), IBranchesRepository
    {
        
    }
}