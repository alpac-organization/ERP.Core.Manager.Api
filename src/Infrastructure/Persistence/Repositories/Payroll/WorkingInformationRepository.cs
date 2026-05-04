using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;
using ERP.Core.Database.Domain.Entities.Payrolls;
namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll
{
    public class WorkingInformationRepository(AppDbContext _context): Repository<WorkingInformation>(_context), IWorkingInformationRepository
    {
        public async Task<WorkingInformation> RegisterWorkingInformation(WorkingInformation workingInformation)
        {
            var informationRegistered = await _context.WorkingInformation.AddAsync(workingInformation);
            return informationRegistered.Entity;
        }
    }
}