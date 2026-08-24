using MediatR;
using Microsoft.Extensions.Logging;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Notifications.v1.Commands;
using ERP.Core.Application.Commons.Interfaces.AWS;

namespace ERP.Core.Manager.Api.Application.Features.Notifications.v1.Handlers
{
    public class UnlinkPushTokenHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ISimpleNotificationServices _notificationServices) : BaseValidatorHandler<UnlinkPushTokenCommand, Unit>(_unitOfWork, _errorManager)
    {
        override public async Task<Unit> Handle(UnlinkPushTokenCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken, true);
            
            if (!access.IsSuccess)
            {
                return access.ErrorResponse;
            }
            

            return Unit.Value;
        }
    }
}