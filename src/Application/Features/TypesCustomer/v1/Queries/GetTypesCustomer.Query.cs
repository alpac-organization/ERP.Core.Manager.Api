using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.TypesCustomer.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.TypesCustomer.v1.Queries
{
    public class GetTypesCustomerQuery : BaseRequest, IRequest<List<TypeCustomerDto>>
    {
        
    }
}