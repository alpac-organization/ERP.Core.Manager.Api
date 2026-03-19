using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUsersRepository Users { get; }
        IModulesRepository Modules { get; }
        ICompaniesRepository Companies { get; }
        IUserProfilesRepository Profiles { get; }
        ISessionsRepository Sessions { get; }
        IRolesRepository Roles { get; }
        IUserModulesRoleRepository UserModules { get; }
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}