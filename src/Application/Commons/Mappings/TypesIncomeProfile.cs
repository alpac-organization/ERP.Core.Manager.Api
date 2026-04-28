using AutoMapper;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class TypesIncomeProfile : Profile
    {
        public TypesIncomeProfile()
        {
            CreateMap<TypesIncome, TypesIncomeDto>()
                .ForMember(dest => dest.TypeIncomeId, src => src.MapFrom(or => or.Id));
        }
    }
}