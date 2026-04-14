using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Database.Domain.Entities.Catalogs;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories
{
    public class CompaniesRepository(AppDbContext _context): Repository<Company>(_context), ICompaniesRepository
    {
        public async Task<List<Company>> GetAvailableCompanies(CancellationToken cancellationToken)
        {
            return await _context.Companies
                .Where(company => company.IsActive == true)
                .ToListAsync(cancellationToken);
        }
    }
}