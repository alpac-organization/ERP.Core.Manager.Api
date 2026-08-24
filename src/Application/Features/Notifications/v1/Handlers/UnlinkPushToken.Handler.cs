using MediatR;
using Microsoft.Extensions.Logging;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Notifications.v1.Commands;
using ERP.Core.Application.Commons.Interfaces.AWS;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Application.Features.Notifications.v1.Handlers
{
    public class UnlinkPushTokenHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ISimpleNotificationServices _notificationServices, ILogger<UnlinkPushTokenHandler> _logger) : BaseValidatorHandler<UnlinkPushTokenCommand, Unit>(_unitOfWork, _errorManager)
    {
        override public async Task<Unit> Handle(UnlinkPushTokenCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken, true);
            
            if (!access.IsSuccess)
            {
                return access.ErrorResponse;
            }

            var device = await _unitOfWork.Devices.Entities
                .Where(dev => dev.FcmToken == request.Token)
                .Where(dev => dev.UserProfileId == access.Profile.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (device is null)
            {   
                _logger.LogInformation("No se encontro el siguiente dispositivo");
                return Unit.Value;   
            }

            await _notificationServices.UnregisterDeviceAsync(device.EndpointArn ?? ""); 

            _logger.LogInformation("Token removido exitosamente");

            return Unit.Value;
        }
    }
}