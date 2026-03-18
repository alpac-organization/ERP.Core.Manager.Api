using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUsersRepository Users { get; }
        IModulesRepository Modules { get; }
        ICompaniesRepository Companies { get; set; }
        IUserProfilesRepository Profiles { get; set; }
        ISessionsRepository Sessions { get; set; }

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}