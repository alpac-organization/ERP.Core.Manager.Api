using MediatR;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Authentication.v1.Handlers
{
    public class LoginWithUsernameAndPasswordHandler(
        IUnitOfWork _unitOfWork, 
        IErrorManager _errorManager
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
                return _errorManager.ThrowBadRequest<LoginDto>("Debe proporcionar un correo o un nombre de usuario.", "IdentityError");
            }

            if (user is null)
            {
                return _errorManager.ThrowBadRequest<LoginDto>("El usuario no se encuentra registrado.", "IdentityError");
            }

            //Verificamos el perfil al que quiere, ingresar
            var profile = await _unitOfWork.Profiles.FirstOrDefaultAsync(
                profile => profile.CompanyId == request.CompanyId && profile.UserId == user.Id, cancellationToken
            );

            if(profile is null)
            {
                return _errorManager.ThrowBadRequest<LoginDto>("Este usuario no tiene un perfil asociado a esta empresa", "IdentityError");
            }


            






            return new ();
        }
    }
}