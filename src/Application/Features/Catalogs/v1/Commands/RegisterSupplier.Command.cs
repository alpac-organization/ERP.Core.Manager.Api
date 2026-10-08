using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands
{
    public class RegisterSupplierCommand : BaseRequest, IRequest<RegisterSupplierDto>
    {
        public string SuppliersLegalName { get; set; } = null!;
        public string? CommercialName { get; set; }
        public string? IdentificationNumber { get; set; }

        public ConstitutionType ConstitutionType { get; set; }
        public IdentificationType IdentificationType { get; set; }

        public SupplierDetails SupplierDetails { get; set; } = new();

        public List<PaymentMethodType> PaymentMethods { get; set; } = [];

        public List<SupplierBankAccountCommand> BankAccounts { get; set; } = [];

        public List<SupplierProductItemDto> Products { get; set; } = [];
    }

    public class SupplierDetails
    {
        public int CreditDays { get; set; }
        public bool HasCredit { get; set; }
        public string? Address { get; set; }
        public string? EmailSupport { get; set; }
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhoneNumber { get; set; }

        public SupplierExclusiveStatus ExclusiveStatus { get; set; } = SupplierExclusiveStatus.None;
        public string? ExclusiveStatusComments { get; set; }

        public SupplierType SupplierType { get; set; } = SupplierType.Ordinary;
        public Currency Currency { get; set; } = Currency.NIO;

        public string? ExclusiveBrandsOrParts { get; set; }
        public decimal? CreditLimit { get; set; }
        public Currency? CreditCurrency { get; set; }
        public int AlertDaysBeforeDue { get; set; }
        public bool ApplyIrRetention { get; set; } = true;
        public bool ApplyMunicipalRetention { get; set; } = true;
        public bool IsTaxExempt { get; set; } = false;
    }

    public class SupplierBankAccountCommand
    {
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public BankAccountType AccountType { get; set; }
        public Currency Currency { get; set; }
        public string AccountHolderName { get; set; } = string.Empty;
        public string? AccountHolderIdentification { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class SupplierProductItemDto
    {
        public Guid ProductId { get; set; }
        public decimal UnitPrice { get; set; }

        public Guid? UnitMeasureId { get; set; }
        public List<TierPriceDto>? TierPrices { get; set; } = [];
    }
}
