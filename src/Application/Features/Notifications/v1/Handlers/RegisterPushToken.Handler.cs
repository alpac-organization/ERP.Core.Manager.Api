using MediatR;
using System.Text.Json;
using Microsoft.Extensions.Logging;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Application.Commons.Interfaces.AWS;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Notifications.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Notifications.v1.Handlers
{
    public class RegisterPushTokenHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ISimpleNotificationServices _notificationServices, ILogger<RegisterPushTokenHandler> _logger) : BaseValidatorHandler<RegisterPushTokenCommand, Unit>(_unitOfWork, _errorManager)
    {
        override public async Task<Unit> Handle(RegisterPushTokenCommand request, CancellationToken cancellationToken)
        {
            // var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);
            
            // if (!access.IsSuccess)
            // {
            //     return access.ErrorResponse;
            // }            

            //Your code here.
            _logger.LogInformation("RegisterPushTokenHandler executed successfully for UserId: {UserId}, CompanyId: {CompanyId}", request.UserId, request.CompanyId);

            //Registramos el token del dispositivo en el servicio de notificaciones.
            // var arnToken = await _notificationServices.RegisterDeviceAsync(request.Token, JsonSerializer.Serialize(""));

            // await _notificationServices.SendPushNotificationAsync("arn:aws:sns:us-east-1:889149078931:endpoint/GCM/ERP-Grupo-Vassalli/9fc95631-ef1f-3576-8659-30cb7beed983", new()
            // {
            //     Title = "Nuevo cambio de salario",
            //     Body = "Se ha realizado un cambio de salario en tu cuenta. Por favor, revisa los detalles en la aplicación.",
            // });

            return Unit.Value;
        }
    }
}