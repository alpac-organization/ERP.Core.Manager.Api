using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Catalogs;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories
{
    public interface ICompaniesRepository : IRepository<Company>
    {
        Task<List<Company>> GetAvailableCompanies(CancellationToken cancellationToken);
    }
}