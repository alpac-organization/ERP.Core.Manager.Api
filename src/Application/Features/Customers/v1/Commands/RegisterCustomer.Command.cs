using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;


namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Commands
{
    public class RegisterCustomerCommand : BaseRequest, IRequest<bool>
    {
        
    }
}