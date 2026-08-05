using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos
{
    public class SupplierInformationDto : SupplierDto
    {
        public SupplierDetailsDto SupplierDetails{ get; set; } = new();
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
    }
}