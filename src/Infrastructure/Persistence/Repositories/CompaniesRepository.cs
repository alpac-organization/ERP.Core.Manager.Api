using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Domain.Entities;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories
{
    public class CompaniesRepository(AppDbContext _context): Repository<Companies>(_context), ICompaniesRepository
    {
        public async Task<List<Companies>> GetAvailableCompanies(CancellationToken cancellationToken)
        {
            return await _context.Companies.ToListAsync(cancellationToken);
        }
    }
}