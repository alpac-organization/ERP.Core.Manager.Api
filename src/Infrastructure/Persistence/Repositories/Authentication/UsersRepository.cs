using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Authentication
{
    public class UsersRepository(AppDbContext _context): Repository<User>(_context), IUsersRepository
    {
        public async Task<User> CreateNewUser(User user)
        {
            var entry = await _context.Users.AddAsync(user);
            return entry.Entity;
        }

        public async Task<IEnumerable<User>> GetActiveUsersByCompany(Guid companyId, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Where(u => u.UserStatus == UserStatus.Active &&
                    u.Profiles.Any(p => p.CompanyId == companyId && p.IsActive))
                .Include(u => u.Profiles.Where(p => p.CompanyId == companyId && p.IsActive))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}