using AutoMapper;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class JobPositionsProfile : Profile
    {
        public JobPositionsProfile()
        {
            CreateMap<JobPosition, JobPositionDto>()
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
                .ForMember(dest => dest.JobPositionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.JobPositionName, opt => opt.MapFrom(src => src.JobPositionName));
        }
    }
}