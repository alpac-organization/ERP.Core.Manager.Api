using ERP.Core.Database.Domain.Entities.Payroll;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll
{
    public class PermitApplicationsRepository(AppDbContext _context): Repository<PermitApplication>(_context), IPermitApplicationsRepository
    {
        public async Task<PermitApplication> CreatePermitApplication(PermitApplication payload)
        {
            var vacationRequestCreated = await _context.PermitApplications.AddAsync(payload);    
            return vacationRequestCreated.Entity;
        }
    }
}   