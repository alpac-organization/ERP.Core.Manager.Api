using AutoMapper;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
   public class PermitFormOptionsProfile : Profile
   {
      public PermitFormOptionsProfile()
      {
         CreateMap<PermitApplicationType, EnumOptionsDtos>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => (int)src))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ToString()));

         CreateMap<PermitBeneficiaryScope, EnumOptionsDtos>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => (int)src))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ToString()));

         CreateMap<FamilyRelationship, EnumOptionsDtos>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => (int)src))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ToString()));

         CreateMap<PermitTypePolicy, PermitTypePolicyDto>()
            .ForMember(dest => dest.PermitTypePolicyId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.PermitBeneficiaryScope, opt => opt.MapFrom(src => src.BeneficiaryScope));
      }
   }
}
