using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Dtos
{
    public class WorkAreaDto
    {
        public Guid WorkAreaId { get; set; }
        public int WorkAreaCode { get; set; }
        public Guid CompanyId { get; set; }
        public string? WorkAreaName { get; set; }
        public string? Descripcion { get; set; }
        
        public List<CostCenterDto> CostCenters { get; set; } = [];
    }
}