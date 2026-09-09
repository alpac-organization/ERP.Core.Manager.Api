using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos
{
    public class SupplierInformationDto : SupplierDto
    {
        public SupplierDetailsDto SupplierDetails{ get; set; } = new();

        public List<SupplierBankAccountDto> BankAccounts { get; set; } = [];
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

        // --- NUEVOS CAMPOS ---
        public bool IsExclusive { get; set; }
        public string? ExclusiveBrandsOrParts { get; set; }
        public decimal? CreditLimit { get; set; }
        public Currency? CreditCurrency { get; set; }
        public int AlertDaysBeforeDue { get; set; }
        public PaymentMethodType PreferredPaymentMethod { get; set; }
        public bool ApplyIrRetention { get; set; }
        public bool ApplyMunicipalRetention { get; set; }
        public bool IsTaxExempt { get; set; }
    }
}