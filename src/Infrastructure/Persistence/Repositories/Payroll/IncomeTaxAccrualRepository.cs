using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll
{
    public class IncomeTaxAccrualRepository(AppDbContext _context): Repository<IncomeTaxAccrual>(_context), IIncomeTaxAccrualRepository
    {
        public async Task<IncomeTaxAccrual> RegisterIncomeTaxAccrual(IncomeTaxAccrual income)
        {
            var IncomeRegistered = await _context.IncomeTaxAccruals.AddAsync(income);
            return IncomeRegistered.Entity;
        }
    }
}   