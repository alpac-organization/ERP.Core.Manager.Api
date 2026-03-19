using ERP.Core.Manager.Api.Domain.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories
{
    public interface IRolesRepository : IRepository<Role>
    {
        Task<Role?> ObtainModuleRoleByUserIdAndModuleId(string moduleCode, Guid userId, CancellationToken cancellationToken);
    }
}