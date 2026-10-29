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
            .ForMember(dest=> dest.Value, opt=> opt.MapFrom(src=> (int) src))
            .ForMember(dest=> dest.Name, opt=> opt.MapFrom(src=> src.ToString()))
            .ForMember(dest=> dest.Label, opt=> opt.MapFrom(src=> PermitOptionsLabels.For(src)));

         
         CreateMap<PermitBeneficiaryScope, EnumOptionsDtos>()
            .ForMember(dest=> dest.Value, opt=> opt.MapFrom(src=> (int) src))
            .ForMember(dest=> dest.Name, opt=> opt.MapFrom(src=> src.ToString()))
            .ForMember(dest=> dest.Label, opt=> opt.MapFrom(src=> PermitOptionsLabels.For(src)));

         CreateMap<FamilyRelationship, EnumOptionsDtos>()
            .ForMember(dest=> dest.Value, opt=> opt.MapFrom(src=> (int) src))
            .ForMember(dest=> dest.Name, opt=> opt.MapFrom(src=> src.ToString()))
            .ForMember(dest=> dest.Label, opt=> opt.MapFrom(src=> PermitOptionsLabels.For(src)));


         CreateMap<PermitTypePolicy, PermitTypePolicyDto>()
            .ForMember(dest => dest.PermitTypePolicyId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.PermitApplicationTypeLabel, opt => opt.MapFrom(src => PermitOptionsLabels.For(src.PermitApplicationType)))
            .ForMember(dest => dest.PermitBeneficiaryScope, opt => opt.MapFrom(src => src.BeneficiaryScope))
            .ForMember(dest => dest.PermitBeneficiaryLabel, opt => opt.MapFrom(src => PermitOptionsLabels.For(src.BeneficiaryScope)))
            .ForMember(dest => dest.FamilyRelationshipLabel, opt => opt.MapFrom(src => PermitOptionsLabels.For(src.FamilyRelationship)));
         
      }
   }
}