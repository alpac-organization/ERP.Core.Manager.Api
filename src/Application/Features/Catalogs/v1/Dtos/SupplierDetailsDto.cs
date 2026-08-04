using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos
{
    public class SupplierInformationDto
    {
        public Guid SupplierId { get; set; }
        public string? SupplierLegalName { get; set; }
        public string? IdentificationNumber { get; set; }
        public IdentificationType? IdentificationType { get; set; }
        public ConstitutionType ConstitutionType { get; set; }      

        public RegisterUserInformation UserInformation { get; set; } = new ();
    }

    public class SupplierDetails
    {
        public string? Address { get; set; }
        public string? EmailSupport { get; set; }

        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhoneNumber { get; set; }
        
        public int CreditDays { get; set; }
        public bool HasCredit { get; set; }
    }
}