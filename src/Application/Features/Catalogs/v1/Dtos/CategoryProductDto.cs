namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos
{
    public class CategoryProductDto
    {
        public bool IsActive {get; set;}
        public Guid CategoryId { get; set; }
        public string? Name {get; set; }
        public string? Code {get; set; }
    }
}
