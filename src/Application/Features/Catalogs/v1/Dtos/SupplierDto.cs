using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos
{
    public class SupplierDto
    {
        public Guid SupplierId { get; set; }
        public string? SupplierLegalName { get; set; }
        public string? IdentificationNumber { get; set; }
        public IdentificationType? IdentificationType { get; set; }
        public ConstitutionType ConstitutionType { get; set; }      

        public RegisterUserInformation UserInformation { get; set; } = new ();
    }

    public class RegisterUserInformation
    {

        public Guid UserId { get; set; }
        public string? UserFullname { get; set; }
        public string? Email { get; set; }
        
        public AreaDto AreaInformation { get; set; } = new ();
    }

    public class AreaDto
    {
        public Guid AreaId { get; set; }
        public int AreaCode { get; set; }
        public string? WorkAreaName { get; set; }
        
    }

    public class  RegisterSupplierDto
    {
        public Guid SupplierId { get; set; }   
    }
}