using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using System.Text.Json.Serialization;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands
{
    public class UpdateSupplierInformationCommand : BaseRequest, IRequest<bool>
    {
        [JsonIgnore]
        public Guid SupplierId { get; set; }

        public string? SuppliersLegalName { get; set; }
        public string? CommercialName { get; set; }
        public string? IdentificationNumber { get; set; }
        public ConstitutionType? ConstitutionType { get; set; }
        public IdentificationType? IdentificationType { get; set; }

        public SupplierDetailsInformation SupplierDetails { get; set; } = new ();
    }

    public class SupplierDetailsInformation
    {
        public int? CreditDays { get; set; }
        public bool? HasCredit { get; set; }
        public string? Address { get; set; }
        public string? EmailSupport { get; set; }
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhoneNumber { get; set; }

        public bool? IsExclusive { get; set; }
        public string? ExclusiveBrandsOrParts { get; set; }
        public decimal? CreditLimit { get; set; }
        public Currency? CreditCurrency { get; set; }
        public int? AlertDaysBeforeDue { get; set; }
        public PaymentMethodType? PreferredPaymentMethod { get; set; }
        public bool? ApplyIrRetention { get; set; }
        public bool? ApplyMunicipalRetention { get; set; }
        public bool? IsTaxExempt { get; set; }
        
    }
}