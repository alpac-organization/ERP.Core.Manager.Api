using AutoMapper;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Database.Domain.Entities.Warehouse;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings;

public class ProductsProfile : Profile
{
    public ProductsProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id));

        CreateMap<CategoryProducts, CategoryProductDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.ParentId))
            .ForMember(dest => dest.SubCategory, opt => opt.MapFrom(src => src.Children.Where(c => c.IsActive).ToList()));
    }
}