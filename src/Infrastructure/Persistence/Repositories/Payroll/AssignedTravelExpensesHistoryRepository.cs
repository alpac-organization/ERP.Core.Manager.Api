using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll
{
    public class AssignedTravelExpensesHistoryRepository(AppDbContext _context): Repository<AssignedTravelExpensesHistory>(_context), IAssignedTravelExpensesHistoryRepository
    {
        public async Task<AssignedTravelExpensesHistory> RegisterAssignedTravelExpensesHistory(AssignedTravelExpensesHistory assigned)
        {
            var history = await _context.AssignedTravelExpensesHistories.AddAsync(assigned);
            return history.Entity;
        }
    }
}   