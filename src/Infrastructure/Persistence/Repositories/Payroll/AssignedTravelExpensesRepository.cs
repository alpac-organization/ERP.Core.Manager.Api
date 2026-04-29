using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll
{
    public class AssignedTravelExpensesRepository(AppDbContext _context): Repository<AssignedTravelExpenses>(_context), IAssignedTravelExpensesRepository
    {
        public async Task<AssignedTravelExpenses> RegisterAssignedTravelExpenses(AssignedTravelExpenses assigned)
        {
            var history = await _context.AssignedTravelExpenses.AddAsync(assigned);
            return history.Entity;
        }
    }
}   