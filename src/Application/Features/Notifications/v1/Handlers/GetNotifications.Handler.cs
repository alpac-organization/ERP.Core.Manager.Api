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
using ERP.Core.Manager.Api.Application.Features.Notifications.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Notifications.v1.Handlers
{
    public class GetNotificationsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager) : BaseValidatorHandler<GetNotificationsQuery, Unit>(_unitOfWork, _errorManager)
    {
        override public async Task<Unit> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken, true);

            if (!access.IsSuccess)
            {
                // _logger.LogWarning("Acceso denegado para registrar token push. UserId: {UserId}", request.UserId);
                return access.ErrorResponse;
            } 

            return Unit.Value;
        }
    }
}