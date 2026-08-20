using MediatR;
using Microsoft.Extensions.Logging;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Notifications.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Notifications.v1.Handlers
{
    public class RegisterPushTokenHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<RegisterPushTokenHandler> _logger) : IRequestHandler<RegisterPushTokenCommand, Unit>
    {
        public async Task<Unit> Handle(RegisterPushTokenCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚩Iniciando registro de token push para el usuario: {UserId} en la empresa: {CompanyId}", request.UserId, request.CompanyId);

            var profile = await _unitOfWork.Profiles
                .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.CompanyId == request.CompanyId, cancellationToken);

            if (profile is null)
            {
                return _errorManager.ThrowBadRequest<Unit>("Este usuario no tiene un perfil asociado a esta empresa", "ERP:ProfileError");
            }

            profile.DeviceToken = request.Token;

            await _unitOfWork.Profiles.UpdateAsync(profile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅Token push registrado con exito en el perfil del usuario: {UserId}", request.UserId);

            return Unit.Value;
        }
    }
}