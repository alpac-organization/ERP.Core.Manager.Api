using ERP.Core.Database.Domain.Entities.Auth;
using ERP.Core.Manager.Api.Domain.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories
{
    public interface IRolesRepository : IRepository<Role>
    {
        Task<Role?> ObtainModuleRoleByUserIdAndModuleId(string moduleCode, Guid userId, CancellationToken cancellationToken);
    }
}