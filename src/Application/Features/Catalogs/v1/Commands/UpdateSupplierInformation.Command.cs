using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands
{
    public class UpdateSupplierInformationCommand : BaseRequest, IRequest<bool>
    {
        public Guid SupplierId { get; set; }

        public string? SuppliersLegalName { get; set; }
        public string? IdentificationNumber { get; set; }
        public ConstitutionType? ConstitutionType { get; set; }
        public IdentificationType? IdentificationType { get; set; }

        public SupplierDetails SupplierDetails { get; set; } = new ();
    }

    public class SupplierDetailsInformation
    {
        public int CreditDays { get; set; }
        public bool? HasCredit { get; set; }
        public string? Address { get; set; }
        public string? EmailSupport { get; set; }
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhoneNumber { get; set; }
    }
}