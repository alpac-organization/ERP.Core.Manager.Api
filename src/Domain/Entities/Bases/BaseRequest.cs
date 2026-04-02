namespace ERP.Core.Manager.Api.Domain.Entities.Bases
{
    public class BaseRequest
    {
        public Guid UserId { get; set; }
        public Guid CompanyId { get; set; }
        public string ModuleCode { get; set; } = string.Empty;
    }
}