using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories
{
    public interface ICompaniesRepository : IRepository<Companies>
    {
        Task<List<Companies>> GetAvailableCompanies(CancellationToken cancellationToken);
    }
}