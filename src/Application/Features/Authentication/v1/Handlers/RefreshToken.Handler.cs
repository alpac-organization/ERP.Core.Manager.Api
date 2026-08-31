using MediatR;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Authentication.v1.Handlers
{
    public class RefreshTokenHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IAuthServices _authServices) : IRequestHandler<RefreshTokenCommand, LoginDto>
    {
        public async Task<LoginDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var companyData = await _unitOfWork.Companies
                .FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken);

            if (companyData is null)
            {
                return _errorManager.ThrowBadRequest<LoginDto>("Esta empresa no esta registrada", "ERP:InvalidCompany");
            }

            var session = await _unitOfWork.Sessions
                .FirstOrDefaultAsync(s => s.RefreshToken == request.RefreshToken && s.CompanyCode == companyData.Code && s.IsActive, cancellationToken);

            if (session == null)
            {
                return _errorManager.ThrowUnauthorized<LoginDto>("Sesión no reconocida.", "ERP:InvalidSession");
            }

            if (!session.IsActive)
            {
                return _errorManager.ThrowUnauthorized<LoginDto>("La sesión ya ha sido cerrada.", "ERP:SessionInactive");
            }

            if (session.ExpiresAt < DateTime.UtcNow)
            {
                session.IsActive = false;
                await _unitOfWork.Sessions.UpdateAsync(session);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                return _errorManager.ThrowUnauthorized<LoginDto>("Su sesión ha expirado por tiempo.", "ERP:ExpiredSession");
            }

            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Id == session.UserId, cancellationToken);
            
            if (user == null)
            {
                return _errorManager.ThrowUnauthorized<LoginDto>("Usuario no encontrado.", "ERP:UserNotFound");
            }

            var profile = await _unitOfWork.Profiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id && p.CompanyId == request.CompanyId, cancellationToken);

            if (profile == null)
            {
                return _errorManager.ThrowBadRequest<LoginDto>("Este usuario no tiene un perfil asociado a esta empresa", "ERP:ProfileError");
            }
            
            var modulesQuery = _unitOfWork.UserModules.Entities
                .Where(m => m.UserProfileId == profile.Id && m.IsActive);

            var userModules = await _unitOfWork.UserModules.ToListAsync(modulesQuery, cancellationToken);

            var modulesWithAccess = userModules
                .Select(m => m.ModuleCode!) 
                .Where(code => !string.IsNullOrEmpty(code))
                .ToList();

            var newRefreshToken = _authServices.GenerateRefreshToken();
            var newAccessToken = _authServices.GenerateAccessToken(user, companyData.Code!, session.Id, modulesWithAccess);
            
            session.RefreshToken = newRefreshToken;
            session.ExpiresAt = DateTime.UtcNow.AddHours(1);

            await _unitOfWork.Sessions.UpdateAsync(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new LoginDto
            {
                UserId = user.Id,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                UserName = user.UserName,
                BranchId = profile.BranchId,
                CompanyInformation = new()
                {
                    CompanyId = profile.CompanyId,
                    CompanyName = companyData.CompanieName
                }
            };
        }
    }
}