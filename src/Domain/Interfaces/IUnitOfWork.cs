using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICompaniesRepository Companies { get; }
        IModulesRepository Modules { get; }
        
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}