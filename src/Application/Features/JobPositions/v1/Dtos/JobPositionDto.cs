namespace ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Dtos
{
    public class JobPositionDto
    {
        public Guid CompanyId { get; set; }
        public Guid JobPositionId { get; set; }
        public string? Description { get; set; }
        public string? JobPositionName { get; set; }
    }
}