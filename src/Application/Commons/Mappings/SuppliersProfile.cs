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
                .ForMember(dest => dest.ConstitutionType, src => src.MapFrom(su => su.ConstitutionType))

                .ForPath(dest => dest.UserInformation.UserId, src => src.MapFrom(su => su.User.Id))
                .ForPath(dest => dest.UserInformation.Email, src => src.MapFrom(su => su.User.Email))
                .ForPath(dest => dest.UserInformation.UserFullname, src => src.MapFrom(su => su.User.Fullname))
                .ForPath(dest => dest.UserInformation.AreaInformation.AreaId, src => src.MapFrom(su => su.User.WorkArea.Id))
                .ForPath(dest => dest.UserInformation.AreaInformation.WorkAreaName, src => src.MapFrom(su => su.User.WorkArea.WorkAreaName))
                .ForPath(dest => dest.UserInformation.AreaInformation.AreaCode, src => src.MapFrom(su => su.User.WorkArea.WorkAreaCode));
            
            CreateMap<Supplier, SupplierInformationDto>()
                .IncludeBase<Supplier, SupplierDto>()

                .ForPath(dest => dest.SupplierDetails.Address, src => src.MapFrom(su => su.SupplierDetails.Address))
                .ForPath(dest => dest.SupplierDetails.ContactEmail, src => src.MapFrom(su => su.SupplierDetails.ContactEmail))
                .ForPath(dest => dest.SupplierDetails.ContactName, src => src.MapFrom(su => su.SupplierDetails.ContactName))
                .ForPath(dest => dest.SupplierDetails.ContactPhoneNumber, src => src.MapFrom(su => su.SupplierDetails.ContactPhoneNumber))
                .ForPath(dest => dest.SupplierDetails.HasCredit, src => src.MapFrom(su => su.SupplierDetails.HasCredit))
                .ForPath(dest => dest.SupplierDetails.CreditDays, src => src.MapFrom(su => su.SupplierDetails.CreditDays));
        }
    }

    public static class SupplierMapper
    {
        public static Supplier ToSupplierEntity(this Commands.RegisterSupplierCommand command, string registerBy)
        {
            return new()
            {
                Id                   = Guid.NewGuid(),
                IsActive             = true,
                UserId               = command.UserId,
                ConstitutionType     = command.ConstitutionType,
                IdentificationType   = command.IdentificationType,
                IdentificationNumber = command.IdentificationNumber,
                SuppliersLegalName   = command.SuppliersLegalName,
            };
        }

        public static SupplierDetails ToSupplierDetails(this Commands.SupplierDetails command, Guid supplierId)
        {
            return new()
            {
                SupplierId = supplierId,
                Address = command.Address,
                ContactEmail = command.ContactEmail,
                ContactName = command.ContactName,
                ContactPhoneNumber = command.ContactPhoneNumber,
                CreditDays = command.CreditDays,
                EmailSupport = command.EmailSupport,
                HasCredit = command.HasCredit,               
            };
        }
    }
}