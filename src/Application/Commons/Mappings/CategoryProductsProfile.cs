using AutoMapper;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Application.Features.CategoryProducts.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings;

public class CategoryProductsProfile : Profile
{
    public CategoryProductsProfile()
    {
        CreateMap<CategoryProducts, CategoryProductDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.ParentId))
            .ForMember(dest => dest.SubCategory, opt => opt.MapFrom(src => src.Children));
    }
}