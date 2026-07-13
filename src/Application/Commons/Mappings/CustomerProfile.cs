using AutoMapper;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Database.Domain.Entities.Warehouse;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Id));

        }
    }
}