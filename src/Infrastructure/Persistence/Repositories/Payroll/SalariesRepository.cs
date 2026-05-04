using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll
{
    public class SalariesRepository(AppDbContext _context): Repository<Salary>(_context), ISalariesRepository
    {
        public async Task<Salary> RegisterSalary(Salary payload)
        {
            var informationRegistered = await _context.Salaries.AddAsync(payload);
            return informationRegistered.Entity;
        }
    }
}