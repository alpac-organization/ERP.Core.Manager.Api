using ERP.Core.Manager.Api.Domain.Entities.Payroll;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll
{
    public class WorkingInformationRepository(AppDbContext _context): Repository<WorkingInformation>(_context), IWorkingInformationRepository
    {
        public async Task<WorkingInformation> RegisterWorkingInformation(WorkingInformation workingInformation, CancellationToken cancellationToken)
        {
            var informationRegistered = await _context.WorkingInformation.AddAsync(workingInformation, cancellationToken);
            
            await _context.SaveChangesAsync(cancellationToken);

            return informationRegistered.Entity;
        }
    }
}