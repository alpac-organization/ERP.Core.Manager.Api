using ERP.Core.Manager.Api.Domain.Entities.Authentication;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IAuthServices
    {
        string GenerateAccessToken(User user, UserProfile profile, Role role);
    }
}