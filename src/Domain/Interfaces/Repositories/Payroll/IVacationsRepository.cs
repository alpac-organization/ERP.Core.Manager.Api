using ERP.Core.Manager.Api.Domain.Entities.Payroll;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll
{
    public interface IVacationsRepository
    {
        Task<Vacation> RegisterVacationControl(Vacation payload, CancellationToken cancellationToken);
    }
}