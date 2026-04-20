using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Catalogs;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Catalogs
{
    public class ValidityDeductionsRepository(AppDbContext _context) : Repository<ValidityDeductions>(_context), IValidityDeductionsRepository
    {
        
    }
}