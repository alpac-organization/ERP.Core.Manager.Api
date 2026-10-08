using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos
{
    public class SupplierInformationDto : SupplierDto
    {
        public SupplierDetailsDto SupplierDetails { get; set; } = new();

        public List<SupplierBankAccountDto> BankAccounts { get; set; } = [];

        public PagedResponse<SupplierLinkedProductDto> Products { get; set; } = new([], 1, 10);
    }

    public class SupplierDetailsDto
    {
        public string? Address { get; set; }
        public string? EmailSupport { get; set; }

        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhoneNumber { get; set; }

        public int CreditDays { get; set; }
        public bool HasCredit { get; set; }

        public SupplierExclusiveStatus ExclusiveStatus { get; set; }
        public string? ExclusiveStatusComments { get; set; }
        public SupplierType SupplierType { get; set; }
        public Currency Currency { get; set; }
        public string? ExclusiveBrandsOrParts { get; set; }
        public decimal? CreditLimit { get; set; }
        public Currency? CreditCurrency { get; set; }
        public int AlertDaysBeforeDue { get; set; }
        public PaymentMethodType PreferredPaymentMethod { get; set; }
        public bool ApplyIrRetention { get; set; }
        public bool ApplyMunicipalRetention { get; set; }
        public bool IsTaxExempt { get; set; }
    }

    public class SupplierLinkedProductDto
    {
        public Guid ProductId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public Guid UnitMeasureId { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime LastPriceUpdate { get; set; }
        public List<TierPriceResponseDto> TierPrices { get; set; } = [];
    }
}
