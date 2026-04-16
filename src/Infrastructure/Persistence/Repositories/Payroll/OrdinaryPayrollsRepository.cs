using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll
{
    public class OrdinaryPayrollsRepository(AppDbContext _context): Repository<OrdinaryPayroll>(_context), IOrdinaryPayrollsRepository
    {
        public async Task<OrdinaryPayroll> RegisterCollaboratorInTheOrdinaryPayroll(OrdinaryPayroll payload)
        {
            var collaboratorRegistered = await _context.OrdinaryPayrolls.AddAsync(payload);
            return collaboratorRegistered.Entity;
        }
    }
}   