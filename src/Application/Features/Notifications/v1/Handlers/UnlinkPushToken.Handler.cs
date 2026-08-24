using MediatR;
using Microsoft.Extensions.Logging;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Notifications.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Notifications.v1.Handlers
{
    public class UnlinkPushTokenHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager) : BaseValidatorHandler<UnlinkPushTokenCommand, Unit>(_unitOfWork, _errorManager)
    {
        override public async Task<Unit> Handle(UnlinkPushTokenCommand request, CancellationToken cancellationToken)
        {
            //your code here

            return Unit.Value;
        }
    }
}