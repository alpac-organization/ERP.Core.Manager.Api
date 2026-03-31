using ERP.Core.Manager.Api.Domain.Commons;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;

namespace ERP.Core.Manager.Api.Domain.Entities.Catalogs
{
    public class Module: BaseEntity<Guid>
    {
        public bool IsActive { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? ModuleName { get; set; }
        public string? PathRedirect { get; set; }

        public virtual ICollection<UserModuleRoles> UserModuleRoles { get; set; } = [];
    }
}