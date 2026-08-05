using MediatR;
using ERP.Core.Database.Domain.Entities.Auth;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Authentication.v1.Handlers
{
    public class LoginWithUsernameAndPasswordHandler(
        IUnitOfWork _unitOfWork, 
        IErrorManager _errorManager,
        IPasswordHasher _passwordHasher,
        IAuthServices _authServices
    ) : IRequestHandler<LoginWithUsernameAndPasswordCommand, LoginDto>
    {
        public async Task<LoginDto> Handle(LoginWithUsernameAndPasswordCommand request, CancellationToken cancellationToken)
        {
            User? user = null;

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                user = await _unitOfWork.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(request.Username))
            {
                user = await _unitOfWork.Users
                    .FirstOrDefaultAsync(u => u.UserName == request.Username, cancellationToken);
            }
            else
            {
                return _errorManager.ThrowBadRequest<LoginDto>("Debe proporcionar un correo o un nomre de usuario.", "IdentityError");
            }

            if (user is null)
            {
                return _errorManager.ThrowBadRequest<LoginDto>("El usuario no se encuentra registrado.", "ERP:IdentityError");
            }

            if (user.UserStatus != UserStatus.Active)
            {
                switch (user.UserStatus)
                {
                    case UserStatus.Locked :
                    {
                        return _errorManager.ThrowBadRequest<LoginDto>("Usuario se encuentra temporalmente bloqueado, comunicar con el area de informatica", "ERP:USER_BLOCKED");  
                    }
                    case UserStatus.Inactive :
                    {
                        return _errorManager.ThrowBadRequest<LoginDto>("Usuario se encuentra temporalmente inactivo, comunicar con el area de informatica", "ERP:USER_UNACTIVE");   
                    }
                }                
            }

            //Verificamos el perfil al que quiere, ingresar
            var profile = await _unitOfWork.Profiles
                .FirstOrDefaultAsync(profile => profile.CompanyId == request.CompanyId && profile.UserId == user.Id, cancellationToken);

            if(profile is null)
            {
                return _errorManager.ThrowBadRequest<LoginDto>("Este usuario no tiene un perfil asociado a esta empresa", "ERP:ProfileError");
            }

            var isPasswordCorrect = _passwordHasher.VerifyPassword(request.Password!, user.PasswordHash!);

            if (isPasswordCorrect is false)
            {
                return _errorManager.ThrowUnauthorized<LoginDto>("Contraseña Incorrecta", "ERP:InvalidPassword");
            }

            var currentActiveSession = await _unitOfWork.Sessions
                .FirstOrDefaultAsync(s => s.UserId == user.Id && s.IsActive, cancellationToken);


            //Cerramos la session antigua y regresamos un nueva session.
            if (currentActiveSession != null)
            {
                currentActiveSession.IsActive = false;
                currentActiveSession.DeletedAt = DateTime.UtcNow;

                await _unitOfWork.Sessions.UpdateAsync(currentActiveSession);
            }

            var company = await _unitOfWork.Companies.FirstOrDefaultAsync(c => c.Id == profile.CompanyId, cancellationToken);

            var modulesQuery = _unitOfWork.UserModules.Entities
                .Where(m => m.UserProfileId == profile.Id && m.IsActive);

            var userModules = await _unitOfWork.UserModules.ToListAsync(modulesQuery, cancellationToken);

            var modulesWithAccess = userModules
                .Select(m => m.ModuleCode!) 
                .Where(code => !string.IsNullOrEmpty(code))
                .ToList();

            // 5. Preparar la nueva sesión
            var newSessionId = Guid.NewGuid();
            var refreshToken = _authServices.GenerateRefreshToken();
            var accessToken = _authServices.GenerateAccessToken(user, company!.Code!, newSessionId, modulesWithAccess);

            var newSession = new Session()
            {
                Id = newSessionId,
                Device = request.SessionDetails?.DeviceName,
                IpAddress = request.SessionDetails?.IpAddress,
                UserId = user.Id,
                RefreshToken = refreshToken,
                CompanyCode = company.Code,
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            await _unitOfWork.Sessions.CreateNewSession(newSession);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new LoginDto
            {
                UserId               = user.Id,
                Email                = user.Email,
                UserName             = user.UserName,
                AccessToken          = accessToken,
                RefreshToken         = refreshToken,
                FullName             = user.Fullname,
                IdentificationNumber = user.IdentificationNumber,
                UserType             = user.UserType.ToString(),
                BranchId             = user.BranchId,
                CompanyInformation = new()
                {
                    CompanyId       = profile.CompanyId,
                    CompanyName     = company?.CompanieName,
                    ImageUrl        = company?.ImageUrl,
                    NeutralImageUrl = company?.NeutralImageUrl,
                    Alias           = company?.Alias
                },
            };
        }
    }
}