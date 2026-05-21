using AutoMapper;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Features.TypesSubsidy.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class TypesSubsidyProfile : Profile
    {
        public TypesSubsidyProfile()
        {
            CreateMap<TypesSubsidy, TypeSubsidyDto>()
                .ForMember(dest => dest.TypeSubsidyId, src => src.MapFrom(or => or.Id))
                .ForMember(dest => dest.Description, src => src.MapFrom(or => or.Description))
                .ForMember(dest => dest.SubsidyName, src => src.MapFrom(or => or.SubsidyName))
                .ForMember(dest => dest.TypeSubsidyCode, src => src.MapFrom(or => or.Code));
        }
    }
}