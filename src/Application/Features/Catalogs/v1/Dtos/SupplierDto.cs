using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos
{
    public class SupplierDto
    {
        public Guid SupplierId { get; set; }
        public string? SupplierLegalName { get; set; }
        public string? CommercialName { get; set; }
        public string? IdentificationNumber { get; set; }
        public IdentificationType? IdentificationType { get; set; }
        public ConstitutionType ConstitutionType { get; set; } 
        public List<SupplierPaymentMethods> SupplierPaymentMethods { get; set; } =  [];
    }

    public class SupplierPaymentMethods
    {
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public PaymentMethodType PaymentMethodType { get; set; }
    }

    public class  RegisterSupplierDto
    {
        public Guid SupplierId { get; set; }   
    }
}