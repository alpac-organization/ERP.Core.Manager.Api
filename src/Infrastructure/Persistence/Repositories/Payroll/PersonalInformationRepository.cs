using ERP.Core.Database.Domain.Entities.Payroll;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll
{
    public class PersonalInformationRepository(AppDbContext _context): Repository<PersonalInformation>(_context), IPersonalInformationRepository
    {
        public async Task<PersonalInformation> RegisterPersonalInformation(PersonalInformation personalInformation, CancellationToken cancellationToken)
        {
            var informationRegistered = await _context.PersonalInformations.AddAsync(personalInformation, cancellationToken);
            
            await _context.SaveChangesAsync(cancellationToken);

            return informationRegistered.Entity;
        }
    }
}