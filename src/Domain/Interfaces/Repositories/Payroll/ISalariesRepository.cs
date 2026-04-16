using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Domain.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll
{
    public interface ISalariesRepository : IRepository<Salary>
    {
        Task<Salary> RegisterSalary(Salary payload, CancellationToken cancellationToken);
    }
}