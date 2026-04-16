using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll
{
    public class PayrollsRepository(AppDbContext _context): Repository<Core.Database.Domain.Entities.Payrolls.Payroll>(_context), IPayrollsRepository
    {
        public async  Task<Core.Database.Domain.Entities.Payrolls.Payroll> InitializePayroll(Core.Database.Domain.Entities.Payrolls.Payroll payroll)
        {
            var collaboratorRegistered = await _context.Payrolls.AddAsync(payroll);
            return collaboratorRegistered.Entity;
        }
    }
}   