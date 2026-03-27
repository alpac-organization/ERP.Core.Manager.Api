using ERP.Core.Manager.Api.Domain.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll
{
    public interface ISalariesRepository : IRepository<Salary>
    {
        Task<Salary> RegisterSalary(Salary payload, CancellationToken cancellationToken);
    }
}