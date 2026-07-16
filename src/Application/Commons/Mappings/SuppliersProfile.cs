using AutoMapper;
using ERP.Core.Database.Domain.Entities.Shopping;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

using Commands = ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class SuppliersProfile : Profile
    {
        public SuppliersProfile()
        {
            CreateMap<Supplier, SupplierDto>()
                .ForMember(dest => dest.SupplierId, src => src.MapFrom(su => su.Id))
                .ForMember(dest => dest.SupplierLegalName, src => src.MapFrom(su => su.SuppliersLegalName))
                .ForMember(dest => dest.IdentificationType, src => src.MapFrom(su => su.IdentificationType))
                .ForMember(dest => dest.IdentificationNumber, src => src.MapFrom(su => su.IdentificationNumber))
                .ForMember(dest => dest.ContactEmail, src => src.MapFrom(su => su.ContactEmail))
                .ForMember(dest => dest.ContactName, src => src.MapFrom(su => su.ContactName))
                .ForMember(dest => dest.ContactPhoneNumber, src => src.MapFrom(su => su.ContactPhoneNumber))
                .ForMember(dest => dest.Address, src => src.MapFrom(su => su.Address));
        }
    }

    public static class SupplierMapper
    {
        public static Supplier ToSupplierEntity(this Commands.RegisterSupplierCommand command, string registerBy)
        {
            return new()
            {
                IsActive             = true,
                RegisterBy           = registerBy,
                Address              = command.Address,
                ContactName          = command.ContactName,
                ConstitutionType     = command.ConstitutionType,
                IdentificationType   = command.IdentificationType,
                IdentificationNumber = command.IdentificationNumber,
                ContactEmail         = command.ContactEmail,
                ContactPhoneNumber   = command.ContactPhoneNumber,
                EmailSupport         = command.EmailSupport,
                SuppliersLegalName   = command.SuppliersLegalName,
            };
        }
    }
    
}