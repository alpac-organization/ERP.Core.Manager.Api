using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries
{
    public class GetSupplierBankAccountsQuery : BaseRequest, IRequest<List<SupplierBankAccountDto>>
    {
        public Guid SupplierId { get; set; }
    }
}
