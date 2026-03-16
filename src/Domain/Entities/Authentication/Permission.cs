using ERP.Core.Manager.Api.Domain.Commons;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Domain.Entities.Authentication
{
    public class Permission : BaseEntity<Guid>
    {
        public PermissionType PermissionType { get; set; }
    }
}