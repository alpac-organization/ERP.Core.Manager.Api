using ERP.Core.Manager.Api.Domain.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll
{
    public interface IVacationsRepository : IRepository<Vacation>
    {
        Task<Vacation> RegisterVacationControl(Vacation payload, CancellationToken cancellationToken);
    }
}