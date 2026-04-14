using ERP.Core.Database.Domain.Entities.Payroll;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll
{
    public class VacationsRepository(AppDbContext _context): Repository<Vacation>(_context), IVacationsRepository
    {
        public async Task<Vacation> RegisterVacationControl(Vacation payload, CancellationToken cancellationToken)
        {
            var vacationRegistered = await _context.Vacations.AddAsync(payload, cancellationToken);
            
            await _context.SaveChangesAsync(cancellationToken);

            return vacationRegistered.Entity;
        }
    }
}