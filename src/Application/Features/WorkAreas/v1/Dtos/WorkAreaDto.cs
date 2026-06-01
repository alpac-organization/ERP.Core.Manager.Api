namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Dtos
{
    public class WorkAreaDto
    {
        public Guid WorkAreaId { get; set; }
        public Guid CompanyId { get; set; }
        public string? WorkAreaName { get; set; }
        public string? Descripcion { get; set; }
    }
}