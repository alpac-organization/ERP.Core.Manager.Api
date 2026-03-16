using AutoMapper;
using ERP.Core.Manager.Api.Application.Features.Companies.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Catalogs;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class CompaniesProfile : Profile
    {
        public CompaniesProfile()
        {
            CreateMap<Company, CompanyDto>()
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanieName))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl));
        }
    }
}