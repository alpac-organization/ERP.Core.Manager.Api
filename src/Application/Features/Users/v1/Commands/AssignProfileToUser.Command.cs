using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Users.v1.Commands
{
    public class AssignProfileToUserCommand : BaseRequest,  IRequest<bool>
    {
        public Guid AssignedUserId { get; set; }
    }
}