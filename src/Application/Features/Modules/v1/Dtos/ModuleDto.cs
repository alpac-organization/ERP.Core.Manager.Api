namespace ERP.Core.Manager.Api.Application.Features.Modules.v1.Dtos
{
    public class ModuleDto
    {
        public int ModuleId { get; set; }
        public string? ModuleName { get; set; }
        public int CompanyId { get; set; }
        public string? Description { get; set; }
        public string? ModuleCode { get; set; }
    }
}