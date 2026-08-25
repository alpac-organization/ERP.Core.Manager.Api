using MediatR;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Application.Commons.Interfaces.AWS;

using ERP.Core.Manager.Api.Application.Commons.Options;
using ERP.Core.Manager.Api.Application.Features.Notifications.v1.Commands;

using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Commons.Mappings;

namespace ERP.Core.Manager.Api.Application.Features.Notifications.v1.Handlers
{
    public class RegisterPushTokenHandler(
        IUnitOfWork _unitOfWork, 
        IErrorManager _errorManager, 
        ISimpleNotificationServices _notificationServices,
        IOptions<NotificationsOptions> _notificationOptions,
        ILogger<RegisterPushTokenHandler> _logger) 
        : BaseValidatorHandler<RegisterPushTokenCommand, Unit>(_unitOfWork, _errorManager)
    {
        override public async Task<Unit> Handle(RegisterPushTokenCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando registro de token push");

            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken, true);

            if (!access.IsSuccess)
            {
                _logger.LogWarning("Acceso denegado para registrar token push. UserId: {UserId}", request.UserId);
                return access.ErrorResponse;
            } 

            var deviceCopy = _notificationOptions.Value.DeviceRegistrationCopies;

            var pushTitle  = deviceCopy.Title;
            var pushBody   = deviceCopy.Body.Replace("{DeviceName}", request.DeviceName ?? "Dispositivo");

            //Registro del dispositivo del usuario a cierto perfil y verificación de notificaciones push.

            var deviceFinded = await _unitOfWork.Devices.Entities
                .Where(device => device.IsActive)
                .Where(device => device.FcmToken == request.Token)
                .Include(device => device.UserProfile)
                    .ThenInclude(device => device.User)
                .FirstOrDefaultAsync(cancellationToken);

            if (deviceFinded is not null)
            {
                //Alguien contiene este dispositivo
                if (deviceFinded.UserProfile.User.Id == request.UserId)
                {
                    //El mismo usuario.
                    if (deviceFinded.UserProfileId == access.Profile.Id)
                    {
                        deviceFinded.DeviceName = request.DeviceName ?? deviceFinded.DeviceName;
                        deviceFinded.IsActive = true;
                    }
                    else
                    {
                        var deviceEntity = DeviceMapper.ToDeviceEntity(request, access.Profile.Id, deviceFinded.EndpointArn ?? "");
                        await _unitOfWork.Devices.RegisterDevice(deviceEntity);
                    }
                }
                else
                {
                    deviceFinded.IsActive = false;

                    var reassignedDevice = DeviceMapper.ToDeviceEntity(request, access.Profile.Id, deviceFinded.EndpointArn ?? "");
                    await _unitOfWork.Devices.RegisterDevice(reassignedDevice);
                }
            }
            else
            {
                //Este token es nuevo, y nadie tiene este dispositivo: sí se crea el ARN endpoint.
                var arnToken = await _notificationServices.RegisterDeviceAsync(request.Token, access.Profile.Id, request.DeviceName, JsonSerializer.Serialize(access.Profile));

                if (arnToken is null)
                {
                    _logger.LogError("Ocurrio un error al registrar el dispositivo");
                    return Unit.Value;
                }

                var newDeviceEntity = DeviceMapper.ToDeviceEntity(request, access.Profile.Id, arnToken);
                await _unitOfWork.Devices.RegisterDevice(newDeviceEntity);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //Notificar al usuario 🔔
            var device = await _unitOfWork.Devices.Entities
                .Where(device => device.UserProfileId == access.Profile.Id)
                .Where(device => device.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            var result = await _notificationServices.SendPushNotificationAsync(device?.EndpointArn ?? "", new()
            {
                Title = pushTitle,
                Body  = pushBody,
            });

            return Unit.Value;
        }
    }
}