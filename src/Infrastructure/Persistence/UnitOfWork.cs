using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
namespace ERP.Core.Manager.Api.Infrastructure.Persistence
{
    public class UnitOfWork(
        AppDbContext _context,
        ICompaniesRepository companiesRepository,
        IModulesRepository modulesRepository,
        IUsersRepository usersRepository,
        IUserProfilesRepository userProfilesRepository,
        ISessionsRepository sessionsRepository
    ) : IUnitOfWork
    {
        public AppDbContext Context => _context;

        #region Repositories

        public ICompaniesRepository Companies { get; set; } = companiesRepository;
        public IModulesRepository Modules => modulesRepository;
        public IUsersRepository Users { get; set; } = usersRepository;
        public IUserProfilesRepository Profiles { get; set; } = userProfilesRepository;
        public ISessionsRepository Sessions { get; set; } = sessionsRepository;

        #endregion

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