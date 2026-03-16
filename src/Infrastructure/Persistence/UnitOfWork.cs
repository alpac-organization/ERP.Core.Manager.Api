using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence
{
    public class UnitOfWork(
        AppDbContext _context,
        ICompaniesRepository companiesRepository,
        IModulesRepository modulesRepository
        
    ) : IUnitOfWork
    {
        public AppDbContext Context => _context;
        public ICompaniesRepository Companies => companiesRepository;
        public IModulesRepository Modules => modulesRepository;


        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}