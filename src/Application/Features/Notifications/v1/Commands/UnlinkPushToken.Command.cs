using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Notifications.v1.Commands
{
    public class UnlinkPushTokenCommand : BaseRequest, IRequest<Unit>
    {
        public string Token { get; set; } = string.Empty;
    }
}