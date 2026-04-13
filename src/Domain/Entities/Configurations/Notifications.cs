using ERP.Core.Manager.Api.Domain.Commons;

namespace ERP.Core.Manager.Api.Domain.Entities.Configurations
{
    public class Notification : BaseEntity<Guid>
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}