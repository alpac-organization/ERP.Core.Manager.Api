using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;
using ERP.Core.Database.Domain.Entities.Payrolls;
namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll
{
    public class WorkPositionsHistoryRepository(AppDbContext _context): Repository<WorkPositionHistory>(_context), IWorkPositionsHistoryRepository
    {
        public async Task<WorkPositionHistory> RegisterHistory(WorkPositionHistory history)
        {
            var informationRegistered = await _context.WorkPositionHistories.AddAsync(history);
            
            return informationRegistered.Entity;
        }
    }
}