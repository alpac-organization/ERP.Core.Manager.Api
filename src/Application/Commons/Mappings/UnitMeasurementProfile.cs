using AutoMapper;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class UnitMeasurementProfile: Profile
    {
        public UnitMeasurementProfile()
        {
            CreateMap<UnitMeasure, UnitMeasureDto>()
                .ForMember(dest => dest.UnitMeasureId, opt => opt.MapFrom(src => src.Id));
        }
    }
}