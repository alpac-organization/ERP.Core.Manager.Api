namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Dtos
{
    public class WorkAreaDto
    {
        public Guid WorkAreaId { get; set; }
        public string WorkAreaCode { get; set; } = default!;
        public Guid CompanyId { get; set; }
        public string? WorkAreaName { get; set; }
        public string? Description { get; set; }        
    }
}