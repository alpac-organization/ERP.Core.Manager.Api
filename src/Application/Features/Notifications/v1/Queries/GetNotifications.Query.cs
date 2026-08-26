using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Notifications.v1.Queries
{
    public class GetNotificationsQuery : BaseRequest, IRequest<Unit>
    {
        
    }
}