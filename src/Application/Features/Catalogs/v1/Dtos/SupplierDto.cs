using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos
{
    public class SupplierDto
    {
        public Guid SupplierId { get; set; }
        public string? SupplierLegalName { get; set; }
        public string? IdentificationNumber { get; set; }

        public string? Address { get; set; }
        public string? EmailSupport { get; set; }
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhoneNumber { get; set; }

        public IdentificationType IdentificationType { get; set; }
        public ConstitutionType ConstitutionType { get; set; }        
    }
}