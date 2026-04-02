using ERP.Core.Manager.Api.Domain.Commons;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;

namespace ERP.Core.Manager.Api.Domain.Entities.Catalogs
{
    public class Company : BaseEntity<Guid>
    {
        public bool IsActive { get; set; }
        public string? Code { get; set; }
        public string? Alias { get; set; }
        public string? ImageUrl { get; set; }
        public string? NeutralImageUrl { get; set; }
        public string? CompanieName { get; set; }


        public virtual ICollection<Catalog> Catalogs { get; set; } = [];
        public virtual ICollection<Collaborator> Collaborators { get; set; } = [];
    }
}