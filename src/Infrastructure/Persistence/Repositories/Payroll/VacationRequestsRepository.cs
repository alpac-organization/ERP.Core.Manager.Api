using ERP.Core.Manager.Api.Domain.Entities.Payroll;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll
{
    public class VacationRequestRepository(AppDbContext _context): Repository<VacationRequest>(_context), IVacationRequestsRepository
    {
        public async Task<VacationRequest> CreateVacationRequest(VacationRequest payload)
        {
            var vacationRequestCreated = await _context.VacationRequests.AddAsync(payload);    
            return vacationRequestCreated.Entity;
        }
    }
}   