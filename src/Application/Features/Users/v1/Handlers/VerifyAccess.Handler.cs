using MediatR;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Dtos;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Application.Features.Authentication.v1.Handlers
{
    public class VerifyAccessHandler(IUnitOfWork _unitOfWork,IErrorManager _errorManager) : IRequestHandler<VerifyAccessCommand, VerifyAccessDto>
    {
        public async Task<VerifyAccessDto> Handle(VerifyAccessCommand request, CancellationToken cancellationToken)
        {
            var company = await _unitOfWork.Companies
                .FirstOrDefaultAsync(company => company.Id == request.CompanyId, cancellationToken);

            if(company is null)
            {
                _errorManager.ThrowBadRequest<VerifyAccessDto>("Esta empresa no esta registrada en nuestro sistema", "ERP:InvalidCompany");
            }

            var result = new VerifyAccessDto();

            var userInformation = await _unitOfWork.Users
                .FirstOrDefaultAsync(user => user.Id == request.UserId, cancellationToken);

            if (userInformation is null)
            {
                return _errorManager.ThrowBadRequest<VerifyAccessDto>("Usuario no encontrado", "ERP:InvalidUser");            
            }

            var userProfile = await _unitOfWork.Profiles
                .FirstOrDefaultAsync(profile => profile.UserId == userInformation.Id && profile.CompanyId == request.CompanyId, cancellationToken);

            if (userProfile is null)
            {
                return _errorManager.ThrowBadRequest<VerifyAccessDto>("El usuario no tiene un perfil asociado a esta empres", "ERP:001");
            }

            var moduleFinded = await _unitOfWork.UserModules
                .FirstOrDefaultAsync(module => module.ModuleCode == request.ModuleCode && module.UserProfileId == userProfile.Id, cancellationToken);

            if(moduleFinded is not null)
            {
                result.Message = "Usuario tiene acceso a este modulo";
                result.HasAccess = true;
            }
            else
            {
                result.Message = "Usuario no tiene acceso a este modulo";
                result.HasAccess = false;
            }

            return result;
        }
    }
}