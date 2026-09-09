using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using System.Text.Json.Serialization;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands
{
    public class AddSupplierBankAccountCommand : BaseRequest, IRequest<SupplierBankAccountDto>
    {
        [JsonIgnore]
        public Guid SupplierId { get; set; }

        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public BankAccountType AccountType { get; set; }
        public Currency Currency { get; set; }
        public string AccountHolderName { get; set; } = string.Empty;
        public string? AccountHolderIdentification { get; set; }
        public bool IsPrimary { get; set; }
    }
}
