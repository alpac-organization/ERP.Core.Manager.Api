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
                .ForMember(dest => dest.CommercialName, src => src.MapFrom(su => su.CommercialName))
                .ForMember(dest => dest.SupplierLegalName, src => src.MapFrom(su => su.SuppliersLegalName))
                .ForMember(dest => dest.ExclusiveStatus, src => src.MapFrom(su => su.SupplierDetails != null ? su.SupplierDetails.ExclusiveStatus : (ERP.Core.Database.Domain.Enums.SupplierExclusiveStatus?)null))
                .ForPath(dest => dest.SupplierPaymentMethods, src => src.MapFrom(su => su.SupplierPaymentMethods));

            CreateMap<SupplierPaymentMethod, SupplierPaymentMethods>();

            CreateMap<SupplierBankAccount, SupplierBankAccountDto>();

            CreateMap<SupplierProduct, SupplierLinkedProductDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Product.Code))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.UnitMeasureId, opt => opt.MapFrom(src => src.Product.UnitMeasureId))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.LastPriceUpdate, opt => opt.MapFrom(src => src.LastPriceUpdate))
                .ForMember(dest => dest.TierPrices, opt => opt.MapFrom(src => src.TierPrices));

            CreateMap<SupplierProductTierPrice, TierPriceResponseDto>()
                .ForMember(dest => dest.TierPriceId, opt => opt.MapFrom(src => src.Id));

            CreateMap<Supplier, SupplierInformationDto>()
                .IncludeBase<Supplier, SupplierDto>()
                .ForMember(dest => dest.BankAccounts, src => src.MapFrom(su => su.SupplierBankAccounts.Where(b => b.DeletedAt == null)))
                .ForMember(dest => dest.Products, src => src.MapFrom(su => su.SupplierProducts.Where(sp => sp.IsActive && sp.DeletedAt == null)))
                .ForPath(dest => dest.SupplierDetails.Address, src => src.MapFrom(su => su.SupplierDetails.Address))
                .ForPath(dest => dest.SupplierDetails.EmailSupport, src => src.MapFrom(su => su.SupplierDetails.EmailSupport))
                .ForPath(dest => dest.SupplierDetails.ContactEmail, src => src.MapFrom(su => su.SupplierDetails.ContactEmail))
                .ForPath(dest => dest.SupplierDetails.ContactName, src => src.MapFrom(su => su.SupplierDetails.ContactName))
                .ForPath(dest => dest.SupplierDetails.ContactPhoneNumber, src => src.MapFrom(su => su.SupplierDetails.ContactPhoneNumber))
                .ForPath(dest => dest.SupplierDetails.HasCredit, src => src.MapFrom(su => su.SupplierDetails.HasCredit))
                .ForPath(dest => dest.SupplierDetails.CreditDays, src => src.MapFrom(su => su.SupplierDetails.CreditDays))
                .ForPath(dest => dest.SupplierDetails.ExclusiveStatus, src => src.MapFrom(su => su.SupplierDetails.ExclusiveStatus))
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
                ExclusiveStatus          = command.ExclusiveStatus,
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
