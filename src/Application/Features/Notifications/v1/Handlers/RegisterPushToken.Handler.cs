using MediatR;
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
    public class RegisterPushTokenHandler(
        IUnitOfWork _unitOfWork, 
        IErrorManager _errorManager, 
        ISimpleNotificationServices _notificationServices, 
        ILogger<RegisterPushTokenHandler> _logger, 
        IOptions<NotificationsOptions> _notificationOptions) 
        : BaseValidatorHandler<RegisterPushTokenCommand, Unit>(_unitOfWork, _errorManager)
    {
        override public async Task<Unit> Handle(RegisterPushTokenCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando registro de token PUSH. UserId: {UserId}, CompanyId: {CompanyId}, DeviceName: {DeviceName}", 
                request.UserId, request.CompanyId, request.DeviceName);

            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken, true);
            
            if (!access.IsSuccess)
            {
                _logger.LogWarning("Acceso denegado para registrar token PUSH. UserId: {UserId}, CompanyId: {CompanyId}, Error: {@ErrorResponse}", 
                    request.UserId, request.CompanyId, access.ErrorResponse);
                return access.ErrorResponse;
            } 

            string? arnToken = null;

            try
            {
                _logger.LogInformation("Registrando dispositivo en AWS SNS para ProfileId: {ProfileId}...", access.Profile.Id);
                
                arnToken = await _notificationServices.RegisterDeviceAsync(request.Token, access.Profile.Id, request.DeviceName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción no controlada al registrar dispositivo en AWS SNS. ProfileId: {ProfileId}, DeviceName: {DeviceName}", 
                    access.Profile.Id, request.DeviceName);
                
                return _errorManager.ThrowInternalError<Unit>("Ocurrió un error al registrar el dispositivo en el servicio de notificaciones.", "ERP:01");
            }

            if (string.IsNullOrWhiteSpace(arnToken))
            {
                _logger.LogError("AWS SNS devolvió un ARN nulo o vacío. ProfileId: {ProfileId}, Token (primeros 10 chars): {TokenSnippet}", 
                    access.Profile.Id, request.Token?.Substring(0, Math.Min(10, request.Token?.Length ?? 0)));

                return _errorManager.ThrowInternalError<Unit>("Ocurrio un error al registrar el token del dispositivo", "ERP:01");
            }

            _logger.LogInformation("Dispositivo registrado exitosamente en AWS SNS. ArnEndpoint: {ArnToken}", arnToken);

            var deviceCopy = _notificationOptions.Value.DeviceRegistrationCopies;
            var pushTitle = deviceCopy.Title;
            var pushBody = deviceCopy.Body.Replace("{DeviceName}", request.DeviceName ?? "Dispositivo");

            try
            {
                _logger.LogInformation("Enviando notificación push de bienvenida a ArnEndpoint: {ArnToken}", arnToken);

                var result = await _notificationServices.SendPushNotificationAsync(arnToken, new()
                {
                    Title = pushTitle,
                    Body = pushBody
                });

                // Si SendPushNotificationAsync retorna un booleano o un objeto con estado, verifícalo aquí:
                _logger.LogInformation("Notificación push enviada a AWS SNS. Resultado: {@Result}, ArnEndpoint: {ArnToken}", result, arnToken);
            }
            catch (Exception ex)
            {
                // Registramos el error de envío pero no detenemos el flujo si el token ya quedó registrado correctamente
                _logger.LogError(ex, "Error al enviar la notificación push de bienvenida al ArnEndpoint: {ArnToken}", arnToken);
            }

            return Unit.Value;
        }
    }
}