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
}