namespace ERP.Core.Manager.Api.Application.Features.TypesSubsidy.v1.Dtos
{
    public class TypeSubsidyDto
    {
        public Guid TypeSubsidyId { get; set; }
        public string? SubsidyName { get; set; }
        public string? Description { get; set; }
        public string? TypeSubsidyCode { get; set; }
    }
}