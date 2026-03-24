using AutoMapper;
using ERP.Core.Manager.Api.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Application.Features.Modules.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class ModulesProfile : Profile
    {
        public ModulesProfile()
        {
            CreateMap<Module, ModuleDto>()
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
                .ForMember(dest => dest.ModuleId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.ModuleName))
                .ForMember(dest => dest.ModuleCode, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
        }
    }
}