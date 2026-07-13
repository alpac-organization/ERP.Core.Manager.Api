using AutoMapper;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class WorkAreaProfile : Profile
    {
        public WorkAreaProfile()
        {
            CreateMap<WorkArea, WorkAreaDto>()
                .ForMember(dest => dest.WorkAreaId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.WorkAreaName, opt => opt.MapFrom(src => src.WorkAreaName))
                .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId));
        }
    }
}