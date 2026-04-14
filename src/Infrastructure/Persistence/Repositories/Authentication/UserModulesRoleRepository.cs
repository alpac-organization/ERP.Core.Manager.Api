using ERP.Core.Database.Domain.Entities.Auth;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Authentication
{
    public class UserModulesRoleRepository(AppDbContext _context): Repository<UserModuleRoles>(_context), IUserModulesRoleRepository
    {
        public async Task<UserModuleRoles> AssignRolesModule(UserModuleRoles entity)
        {
            var assigSaved = await _context.UserModuleRoles.AddAsync(entity);

            return assigSaved.Entity;
        }
    }
}