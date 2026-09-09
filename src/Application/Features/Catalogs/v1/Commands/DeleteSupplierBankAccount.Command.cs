using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using System.Text.Json.Serialization;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands
{
    public class DeleteSupplierBankAccountCommand : BaseRequest, IRequest<bool>
    {
        [JsonIgnore]
        public Guid SupplierId { get; set; }

        [JsonIgnore]
        public Guid BankAccountId { get; set; }
    }
}
