using ERP.Core.Manager.Api.Domain.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories
{
    public interface IUserModulesRoleRepository : IRepository<UserModuleRoles>
    {
        Task AssignRolesModule(Guid RoleId, string ModuleCode, Guid ProfileId);
    }
}