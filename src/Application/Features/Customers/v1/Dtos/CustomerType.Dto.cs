namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Dtos
{
    public record RegisterCustomerTypeDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}