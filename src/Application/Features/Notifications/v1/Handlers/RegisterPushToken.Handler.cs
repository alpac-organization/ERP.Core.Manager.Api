using MediatR;
using Microsoft.Extensions.Logging;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Notifications.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Notifications.v1.Handlers
{
    public class RegisterPushTokenHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<RegisterPushTokenHandler> _logger) : BaseValidatorHandler<RegisterPushTokenCommand, Unit>(_unitOfWork, _errorManager)
    {
        override public async Task<Unit> Handle(RegisterPushTokenCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);
            
            if (!access.IsSuccess)
            {
                return access.ErrorResponse;
            }            

            //Your code here.
            _logger.LogInformation("RegisterPushTokenHandler executed successfully for UserId: {UserId}, CompanyId: {CompanyId}", request.UserId, request.CompanyId);

            return Unit.Value;
        }
    }
}