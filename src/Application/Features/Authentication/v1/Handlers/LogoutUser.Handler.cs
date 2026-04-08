using MediatR;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Authentication.v1.Handlers
{
    public class LogoutUserHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager) : IRequestHandler<LogoutUserCommand, bool>
    {
        public async Task<bool> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
        {
            var companyData = await _unitOfWork.Companies
                .FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken);

            if (companyData is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Esta empresa no esta registrada", "ERP:02");
            }

            var session = await _unitOfWork.Sessions
                .FirstOrDefaultAsync(s => s.RefreshToken == request.RefreshToken && s.CompanyCode == companyData.Code && s.IsActive, cancellationToken);

            if (session == null)
            {
                return _errorManager.ThrowUnauthorized<bool>("Sesión no reconocida.", "ERP:03");
            }

            if (!session.IsActive)
            {
                return _errorManager.ThrowUnauthorized<bool>("La sesión ya ha sido cerrada.", "ERP:04");
            }

            var currentSession = await _unitOfWork.Sessions
                .FirstOrDefaultAsync(s => 
                    s.RefreshToken == request.RefreshToken && 
                    s.CompanyCode == companyData.Code && 
                    s.IsActive, 
                    cancellationToken);

            if (session == null)
            {
                return _errorManager.ThrowUnauthorized<bool>("No se encontró una sesión activa para cerrar.", "ERP:05");
            }

            session.IsActive = false;
            session.DeletedAt = DateTime.UtcNow;

            await _unitOfWork.Sessions.UpdateAsync(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}