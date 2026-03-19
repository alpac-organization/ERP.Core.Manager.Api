using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Authentication
{
    public class UserModulesRoleRepository(AppDbContext _context): Repository<UserModuleRoles>(_context), IUserModulesRoleRepository
    {
        public async Task<UserModuleRoles> AssignRolesModule(Guid RoleId, string ModuleCode, Guid ProfileId)
        {
            var entity = new UserModuleRoles()
            {
                ModuleCode = ModuleCode,
                RoleId = RoleId,
                UserProfileId = ProfileId,
                IsActive = true               
            };

            var assigSaved = await _context.UserModuleRoles.AddAsync(entity);

            return assigSaved.Entity;
        }
    }
}