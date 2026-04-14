using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Entities.Auth;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Authentication
{
    public class RolesRepository(AppDbContext _context): Repository<Role>(_context), IRolesRepository
    {
        public async Task<Role?> ObtainModuleRoleByUserIdAndModuleId(string moduleCode, Guid userId, CancellationToken cancellationToken)
        {
            var query = from profile in _context.Profiles
                join module in _context.ModulesWithRoles 
                    on profile.Id equals module.UserProfileId
                join role in _context.Roles 
                    on module.RoleId equals role.Id
                where profile.UserId == userId && module.ModuleCode == moduleCode
                select role;

            return await query.FirstOrDefaultAsync(cancellationToken);
        }       
    }
}