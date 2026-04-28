using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll
{
    public class IncomesRepository(AppDbContext _context): Repository<Income>(_context), IIncomesRepository
    {
        public async  Task<Income> RegisterIncome(Income income)
        {
            var IncomeRegistered = await _context.Incomes.AddAsync(income);
            return IncomeRegistered.Entity;
        }
    }
}   