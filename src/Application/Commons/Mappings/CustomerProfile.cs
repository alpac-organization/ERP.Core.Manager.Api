using AutoMapper;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Database.Domain.Entities.Warehouse;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Commands;
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

            CreateMap<RegisterCustomerCommand, Customer>()
                .ForMember(d => d.Id, o => o.MapFrom(_ => Guid.NewGuid()))
                .ForMember(d => d.Cif, o => o.MapFrom(s => s.Cif))
                .ForMember(d => d.LegalName, o => o.MapFrom(s => s.LegalName))
                .ForMember(d => d.PictureUrl, o => o.Ignore())
                .ForMember(d => d.IdentificationNumber, o => o.MapFrom(s => s.IdentificationNumber))
                .ForMember(d => d.IdentificationType, o => o.MapFrom(s => s.IdentificationType))
                .ForMember(d => d.CustomerTypeId, o => o.MapFrom(s => s.CustomerTypeId))
                .ForMember(d => d.CompanyId, o => o.MapFrom(s => s.CompanyId))
                .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
                .ForMember(d => d.CustomerType, o => o.Ignore())
                .ForMember(d => d.Company, o => o.Ignore())
                .ForMember(d => d.ServiceOrders, o => o.Ignore())
                .ForMember(d => d.PurchaseRequests, o => o.Ignore());

            CreateMap<CustomerType, CustomerTypeDto>()
                .ForMember(dest => dest.CustomerTypeId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            CreateMap<RegisterCustomerTypeCommand, CustomerType>()
                .ForMember(d => d.Id, o => o.MapFrom(_ => Guid.NewGuid()))
                .ForMember(d => d.Code, o => o.MapFrom(s => s.Code))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
                .ForMember(d => d.Customers, o => o.Ignore());
        }
    }

    public static class CustomerMapper
    {
        public static RegisterCustomerCommand ToCommand(
            this RegisterCustomerDto dto,
            Guid userId,
            Guid companyId,
            string moduleCode)
        {
            return new()
            {
                UserId = userId,
                CompanyId = companyId,
                ModuleCode = moduleCode,
                Cif = dto.Cif,
                LegalName = dto.LegalName,
                PictureBase64 = dto.PictureBase64,
                IdentificationNumber = dto.IdentificationNumber,
                IdentificationType = dto.IdentificationType,
                CustomerTypeId = dto.CustomerTypeId
            };
        }
    }

    public static class CustomerTypeMapper
    {
        public static RegisterCustomerTypeCommand ToCommand(
            this RegisterCustomerTypeDto dto,
            Guid userId,
            Guid companyId,
            string moduleCode)
        {
            return new()
            {
                UserId = userId,
                CompanyId = companyId,
                ModuleCode = moduleCode,
                Code = dto.Code,
                Name = dto.Name
            };
        }
    }
}