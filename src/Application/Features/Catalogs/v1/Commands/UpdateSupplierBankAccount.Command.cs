using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using System.Text.Json.Serialization;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands
{
    public class UpdateSupplierBankAccountCommand : BaseRequest, IRequest<bool>
    {
        [JsonIgnore]
        public Guid SupplierId { get; set; }

        [JsonIgnore]
        public Guid BankAccountId { get; set; }

        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public BankAccountType? AccountType { get; set; }
        public Currency? Currency { get; set; }
        public string? AccountHolderName { get; set; }
        public string? AccountHolderIdentification { get; set; }
        public bool? IsPrimary { get; set; }
    }
}
