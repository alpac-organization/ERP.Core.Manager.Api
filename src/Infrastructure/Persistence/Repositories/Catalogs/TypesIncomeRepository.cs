using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Catalogs;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Catalogs
{
    public class TypesIncomeRepository(AppDbContext _context) : Repository<TypesIncome>(_context), ITypesIncomeRepository
    {
        
    }
}