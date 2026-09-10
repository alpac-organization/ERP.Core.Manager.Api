using AutoMapper;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class CostCenterProfile : Profile
    {
        public CostCenterProfile()
        {
            CreateMap<CostCenter, CostCenterDto>()
                .ForMember(dest => dest.CostCenterId, opt => opt.MapFrom(src => src.Id));
        }
    }
}