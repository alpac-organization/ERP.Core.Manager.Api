using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Database.Domain.Entities.Auth;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Authentication
{
    public class UserProfilesRepository(AppDbContext _context): Repository<UserProfile>(_context), IUserProfilesRepository
    {
        public async Task<UserProfile> CreateNewUserProfile(UserProfile profile)
        {
            var entry = await _context.Profiles.AddAsync(profile);
            
            return entry.Entity;
        }
    }
}