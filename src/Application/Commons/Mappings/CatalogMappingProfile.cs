using AutoMapper;
using ERP.Core.Manager.Api.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Mappings
{
    public class CatalogMappingProfile : Profile
    {
        public CatalogMappingProfile()
        {
            CreateMap<SubCatalog, CatalogDetailsDto>()
                .ForMember(dest => dest.SubCatalogId, opt => opt.MapFrom(src => src.Id));
        }
    }
}