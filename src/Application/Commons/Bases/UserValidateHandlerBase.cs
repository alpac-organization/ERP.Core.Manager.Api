using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;
using ERP.Core.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Application.Commons.Bases
{
    public abstract class UserValidateHandlerBase(IUnitOfWork unitOfWork, IErrorManager errorManager)
    {
        protected readonly IUnitOfWork _unitOfWork = unitOfWork;
        protected readonly IErrorManager _errorManager = errorManager;

        protected async Task<UserProfile> ValidateUserAndProfileAsync(Guid userId, Guid companyId, CancellationToken ct)
        {
            var userExists = await _unitOfWork.Users.Entities.AnyAsync(u => u.Id == userId, ct);
            
            if (!userExists)
            {
                _errorManager.ThrowBadRequest<bool>("Este usuario no existe!", "ERP:001");
            }

            var profile = await _unitOfWork.Profiles
                .FirstOrDefaultAsync(profile => profile.UserId == userId && profile.CompanyId == companyId, ct);

            if (profile is null)
            {
                _errorManager.ThrowBadRequest<bool>("Este usuario no tiene un perfil válido para esta empresa", "ERP:002");
            }

            return profile!;
        }
    }
    
}