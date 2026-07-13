using AutoMapper;
using ERP.Core.Database.Domain.Entities.Warehouse;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.LegalName, opt => opt.MapFrom(src => src.LegalName))
                .ForMember(dest => dest.IdentificationType, opt => opt.MapFrom(src => src.IdentificationType))
                .ForMember(dest => dest.IdentificationNumber, opt => opt.MapFrom(src => src.IdentificationNumber));
        }
    }
}