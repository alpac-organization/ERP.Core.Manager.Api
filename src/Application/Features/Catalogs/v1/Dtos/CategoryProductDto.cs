namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos
{
    public class CategoryProductDto
    {
        public Guid Id { get; set; }
        public string Name {get; set;} = null!;
        public string? Code {get; set;}
        public bool IsActive {get; set;}
        public Guid? CategoryId {get; set;}
        public List<CategoryProductDto> SubCategory {get;set;} = [];
    }
}
