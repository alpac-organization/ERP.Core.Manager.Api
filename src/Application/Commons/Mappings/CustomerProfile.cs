using AutoMapper;
using ERP.Core.Database.Domain.Entities.Operations;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customers, CustomerDto>()
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.LegalName, opt => opt.MapFrom(src => src.LegalName))
                .ForMember(dest => dest.IdentificationType, opt => opt.MapFrom(src => src.IdentificationType))
                .ForMember(dest => dest.IdentificationNumber, opt => opt.MapFrom(src => src.IdentificationNumber));

            CreateMap<RegisterCustomerCommand, Customers>()
                .ForMember(d => d.Id, o => o.MapFrom(_ => Guid.NewGuid()))
                .ForMember(d => d.LegalName, o => o.MapFrom(s => s.LegalName))
                .ForMember(d => d.IdentificationNumber, o => o.MapFrom(s => s.IdentificationNumber))
                .ForMember(d => d.IdentificationType, o => o.MapFrom(s => s.IdentificationType))
                .ForMember(d => d.CompanyId, o => o.MapFrom(s => s.CompanyId))
                .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
                .ForMember(d => d.CustomerType, o => o.Ignore())
                .ForMember(d => d.Company, o => o.Ignore());
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