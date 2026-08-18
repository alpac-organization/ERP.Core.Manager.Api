using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Commands
{
    public class RegisterCustomerTypeCommand : BaseRequest, IRequest<bool>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}