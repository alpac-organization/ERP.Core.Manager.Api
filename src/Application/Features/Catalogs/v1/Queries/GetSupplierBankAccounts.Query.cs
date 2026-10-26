using MediatR;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries
{
    public class GetSupplierBankAccountsQuery : BaseRequest, IRequest<List<SupplierBankAccountDto>>
    {
        public Guid SupplierId { get; set; }
    }
}
