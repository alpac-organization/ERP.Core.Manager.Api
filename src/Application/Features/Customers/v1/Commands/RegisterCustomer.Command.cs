using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Database.Domain.Enums;


namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Commands
{
    public class RegisterCustomerCommand : BaseRequest, IRequest<bool>
    {
        public string Cif { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string? PictureBase64 { get; set; }
        public string IdentificationNumber { get; set; } = string.Empty;
        public IdentificationType IdentificationType { get; set; }
        public Guid CustomerTypeId { get; set; }
    }
}