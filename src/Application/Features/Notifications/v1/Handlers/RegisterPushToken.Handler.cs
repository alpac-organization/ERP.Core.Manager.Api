using MediatR;
using System.Text.Json;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Options;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Application.Commons.Interfaces.AWS;
using ERP.Core.Manager.Api.Application.Commons.Options;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Notifications.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Notifications.v1.Handlers
{
    public class RegisterPushTokenHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ISimpleNotificationServices _notificationServices, ILogger<RegisterPushTokenHandler> _logger, IOptions<NotificationsOptions> _notificationOptions) : BaseValidatorHandler<RegisterPushTokenCommand, Unit>(_unitOfWork, _errorManager)
    {
        override public async Task<Unit> Handle(RegisterPushTokenCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken, true);
            
            if (!access.IsSuccess)
            {
                return access.ErrorResponse;
            } 

            _logger.LogInformation("RegisterPushTokenHandler executed successfully for UserId: {UserId}, CompanyId: {CompanyId}", request.UserId, request.CompanyId);
            
            var arnToken = await _notificationServices.RegisterDeviceAsync(request.Token, access.Profile.Id, request.DeviceName);

            if (arnToken is null)
            {
                return _errorManager.ThrowInternalError<Unit>("Ocurrio un error al registrar el token del dispositivo", "ERP:01");
            }

            var deviceCopy = _notificationOptions.Value.DeviceRegistrationCopies;

            await _notificationServices.SendPushNotificationAsync(arnToken, new()
            {
                Title = deviceCopy.Title,
                Body = deviceCopy.Body.Replace("{DeviceName}", request.DeviceName ?? "Dispositivo")
            });

            return Unit.Value;
        }
    }
}