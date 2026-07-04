using ERP.Core.Manager.Api.Application.Features.Customers.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Queries
{
    public class GetCustomersAvailableQuery : BaseRequest, IRequest<List<CustomerDto>>
    {
        
    }
}