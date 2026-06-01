namespace ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Dtos
{
    public class CostCenterDto
    {
        public Guid AreaId { get; set;}
        public Guid CostCenterId { get; set; }
        public string? Descripcion { get; set; }
        public string? CostCenterName { get; set; }
    }
}