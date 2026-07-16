using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands
{
    public class RegisterSupplierCommand : BaseRequest, IRequest<bool>
    {
        public string SuppliersLegalName { get; set; } = null!;
        public string IdentificationNumber { get; set; } = null!;

        public ConstitutionType ConstitutionType { get; set; }
        public IdentificationType IdentificationType { get; set; }

        public string? Address { get; set; }
        public string? EmailSupport { get; set; }
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhoneNumber { get; set; }
    }
}