using MediatR;
using Microsoft.Extensions.Logging;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Notifications.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Notifications.v1.Handlers
{
    public class UnlinkPushTokenHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<UnlinkPushTokenHandler> _logger) : IRequestHandler<UnlinkPushTokenCommand, Unit>
    {
        public async Task<Unit> Handle(UnlinkPushTokenCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚩Iniciando desvinculacion de token push para el usuario: {UserId} en la empresa: {CompanyId}", request.UserId, request.CompanyId);

            var profile = await _unitOfWork.Profiles
                .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.CompanyId == request.CompanyId, cancellationToken);

            if (profile is null)
            {
                return _errorManager.ThrowBadRequest<Unit>("Este usuario no tiene un perfil asociado a esta empresa", "ERP:ProfileError");
            }

            if (profile.DeviceToken != request.Token)
            {
                _logger.LogWarning("⚠️El token proporcionado no coincide con el registrado para el usuario: {UserId}", request.UserId);

                return Unit.Value;
            }

            profile.DeviceToken = string.Empty;

            await _unitOfWork.Profiles.UpdateAsync(profile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅Token push desvinculado con exito del perfil del usuario: {UserId}", request.UserId);

            return Unit.Value;
        }
    }
}