using ERP.Core.Database.Domain.Entities.Auth;
using ERP.Core.Manager.Api.Domain.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories
{
    public interface IUserModulesRoleRepository : IRepository<UserModuleRoles>
    {
        Task<UserModuleRoles> AssignRolesModule(UserModuleRoles entity);
    }
}