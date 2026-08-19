using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Queries
{
    public class GetCustomerTypesQuery : BaseRequest, IRequest<List<CustomerTypeDto>>
    {
        public bool? Status { get; set; }
    }
}