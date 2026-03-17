using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Domain.Interfaces
{
    public interface IErrorManager : IDisposable
    {
        ICompaniesRepository Companies { get; }
        IModulesRepository Modules { get; }
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}