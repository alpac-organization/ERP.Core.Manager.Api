using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Queries
{
    public class GetCustomersAvailableQuery : BaseRequest, IRequest<List<CustomerDto>>
    {
        public bool? Status { get; set; }
        public Guid? CustomerTypeId { get; set; }
    }
}