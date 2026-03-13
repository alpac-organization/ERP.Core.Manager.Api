using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities
{
    public class Modules : BaseEntity<Guid>
    {
        public string ModuleName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public Guid CompanieId { get; set; }
    }
}