using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Authentication
{
    public class UserProfilesRepository(AppDbContext _context): Repository<UserProfile>(_context), IUserProfilesRepository
    {
        public async Task<UserProfile> CreateNewUserProfile(UserProfile profile, CancellationToken cancellationToken)
        {
            var entry = await _context.Profiles.AddAsync(profile, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return entry.Entity;
        }
    }
}