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
                .ForMember(dest => dest.CommercialName, src => src.MapFrom(su => su.CommercialName))
                .ForMember(dest => dest.IdentificationType, src => src.MapFrom(su => su.IdentificationType))
                .ForMember(dest => dest.IdentificationNumber, src => src.MapFrom(su => su.IdentificationNumber))
                .ForMember(dest => dest.ConstitutionType, src => src.MapFrom(su => su.ConstitutionType))

                .ForPath(dest => dest.UserInformation.UserId, src => src.MapFrom(su => su.User.Id))
                .ForPath(dest => dest.UserInformation.Email, src => src.MapFrom(su => su.User.Email))
                .ForPath(dest => dest.UserInformation.UserFullname, src => src.MapFrom(su => su.User.Fullname))
                .ForPath(dest => dest.UserInformation.AreaInformation.AreaId, src => src.MapFrom(su => su.User.WorkArea.Id))
                .ForPath(dest => dest.UserInformation.AreaInformation.WorkAreaName, src => src.MapFrom(su => su.User.WorkArea.WorkAreaName))
                .ForPath(dest => dest.UserInformation.AreaInformation.AreaCode, src => src.MapFrom(su => su.User.WorkArea.WorkAreaCode));
            
            CreateMap<SupplierBankAccount, SupplierBankAccountDto>();

            CreateMap<Supplier, SupplierInformationDto>()
                .IncludeBase<Supplier, SupplierDto>()
                .ForMember(dest => dest.BankAccounts, src => src.MapFrom(su => su.SupplierBankAccounts.Where(b => b.DeletedAt == null)))
                .ForPath(dest => dest.SupplierDetails.Address, src => src.MapFrom(su => su.SupplierDetails.Address))
                .ForPath(dest => dest.SupplierDetails.EmailSupport, src => src.MapFrom(su => su.SupplierDetails.EmailSupport))
                .ForPath(dest => dest.SupplierDetails.ContactEmail, src => src.MapFrom(su => su.SupplierDetails.ContactEmail))
                .ForPath(dest => dest.SupplierDetails.ContactName, src => src.MapFrom(su => su.SupplierDetails.ContactName))
                .ForPath(dest => dest.SupplierDetails.ContactPhoneNumber, src => src.MapFrom(su => su.SupplierDetails.ContactPhoneNumber))
                .ForPath(dest => dest.SupplierDetails.HasCredit, src => src.MapFrom(su => su.SupplierDetails.HasCredit))
                .ForPath(dest => dest.SupplierDetails.CreditDays, src => src.MapFrom(su => su.SupplierDetails.CreditDays))
                .ForPath(dest => dest.SupplierDetails.IsExclusive, src => src.MapFrom(su => su.SupplierDetails.IsExclusive))
                .ForPath(dest => dest.SupplierDetails.ExclusiveBrandsOrParts, src => src.MapFrom(su => su.SupplierDetails.ExclusiveBrandsOrParts))
                .ForPath(dest => dest.SupplierDetails.CreditLimit, src => src.MapFrom(su => su.SupplierDetails.CreditLimit))
                .ForPath(dest => dest.SupplierDetails.CreditCurrency, src => src.MapFrom(su => su.SupplierDetails.CreditCurrency))
                .ForPath(dest => dest.SupplierDetails.AlertDaysBeforeDue, src => src.MapFrom(su => su.SupplierDetails.AlertDaysBeforeDue))
                .ForPath(dest => dest.SupplierDetails.PreferredPaymentMethod, src => src.MapFrom(su => su.SupplierDetails.PreferredPaymentMethod))
                .ForPath(dest => dest.SupplierDetails.ApplyIrRetention, src => src.MapFrom(su => su.SupplierDetails.ApplyIrRetention))
                .ForPath(dest => dest.SupplierDetails.ApplyMunicipalRetention, src => src.MapFrom(su => su.SupplierDetails.ApplyMunicipalRetention))
                .ForPath(dest => dest.SupplierDetails.IsTaxExempt, src => src.MapFrom(su => su.SupplierDetails.IsTaxExempt));
        }
    }

    public static class SupplierMapper
    {
        public static Supplier ToSupplierEntity(this Commands.RegisterSupplierCommand command, string registerBy)
        {
            var supplierId = Guid.NewGuid();
            return new()
            {
                Id                   = supplierId,
                IsActive             = true,
                UserId               = command.UserId,
                ConstitutionType     = command.ConstitutionType,
                IdentificationType   = command.IdentificationType,
                IdentificationNumber = command.IdentificationNumber,
                SuppliersLegalName   = command.SuppliersLegalName,
                CommercialName       = command.CommercialName,
                SupplierBankAccounts = command.BankAccounts?.Select(b => b.ToSupplierBankAccount(supplierId)).ToList() ?? []
            };
        }

        public static SupplierDetails ToSupplierDetails(this Commands.SupplierDetails command, Guid supplierId)
        {
            return new()
            {
                SupplierId               = supplierId,
                Address                  = command.Address,
                ContactEmail             = command.ContactEmail,
                ContactName              = command.ContactName,
                ContactPhoneNumber       = command.ContactPhoneNumber,
                CreditDays               = command.CreditDays,
                EmailSupport             = command.EmailSupport,
                HasCredit                = command.HasCredit,
                IsExclusive              = command.IsExclusive,
                ExclusiveBrandsOrParts   = command.ExclusiveBrandsOrParts,
                CreditLimit              = command.CreditLimit,
                CreditCurrency           = command.CreditCurrency,
                AlertDaysBeforeDue       = command.AlertDaysBeforeDue,
                PreferredPaymentMethod   = command.PreferredPaymentMethod,
                ApplyIrRetention         = command.ApplyIrRetention,
                ApplyMunicipalRetention  = command.ApplyMunicipalRetention,
                IsTaxExempt              = command.IsTaxExempt
            };
        }

        public static SupplierBankAccount ToSupplierBankAccount(this Commands.SupplierBankAccountCommand command, Guid supplierId)
        {
            return new()
            {
                SupplierId                  = supplierId,
                BankName                    = command.BankName,
                AccountNumber               = command.AccountNumber,
                AccountType                 = command.AccountType,
                Currency                    = command.Currency,
                AccountHolderName           = command.AccountHolderName,
                AccountHolderIdentification = command.AccountHolderIdentification,
                IsPrimary                   = command.IsPrimary
            };
        }
    }
}