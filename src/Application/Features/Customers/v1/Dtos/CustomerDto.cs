using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Dtos
{
    public class CustomerDto
    {
        public Guid CustomerId { get; set; }
        public string? LegalName { get; set; }

        public string? IdentificationNumber { get; set; }
        public IdentificationType IdentificationType { get; set; }
    }
    public record RegisterCustomerDto
    {
        public string Cif { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string? PictureBase64 { get; set; }
        public string IdentificationNumber { get; set; } = string.Empty;
        public IdentificationType IdentificationType { get; set; }
        public Guid CustomerTypeId { get; set; }
    }
    public class CustomerTypeDto
    {
        public Guid CustomerTypeId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
