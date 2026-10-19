using AutoMapper;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
   public class FaultsProfile : Profile
   {
      public FaultsProfile()
      {
         CreateMap<FaultCatalog, FaultCatalogDto>()
            .ForMember(dest => dest.FaultCatalogId, opt => opt.MapFrom(src => src.Id));
      }
   }
}