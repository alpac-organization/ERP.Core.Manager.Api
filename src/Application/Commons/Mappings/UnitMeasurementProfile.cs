using AutoMapper;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Modules.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class UnitMeasurementProfile: Profile
    {
        public UnitMeasurementProfile()
        {
            CreateMap<UnitMeasure, UnitMeasureDto>()
                .ForMember(dest => dest.UnitMeasureId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Symbol))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code));
        }
    }
}