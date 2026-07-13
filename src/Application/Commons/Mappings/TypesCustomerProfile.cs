using AutoMapper;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Application.Features.TypesCustomer.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class TypesCustomerProfile : Profile
    {
        public TypesCustomerProfile()
        {
            CreateMap<CustomerType, TypeCustomerDto>()
                .ForMember(dest => dest.Code, src => src.MapFrom(or => or.Code))
                .ForMember(dest => dest.TypeCustomerId, src => src.MapFrom(or => or.Id))
                .ForMember(dest => dest.TypeCustomerName, src => src.MapFrom(or => or.Name));
        }
    }
}