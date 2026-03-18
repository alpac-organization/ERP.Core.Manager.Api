using ERP.Core.Manager.Api.Domain.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Define el contrato para las operaciones de persistencia de los perfiles de usuario.
    /// Esta interfaz gestiona la vinculación entre la identidad del usuario y su contexto dentro de una empresa.
    /// </summary>
    public interface IUserProfilesRepository : IRepository<UserProfile>
    {
        /// <summary>
        /// Registra un nuevo perfil para un usuario, estableciendo su relación con una empresa (Tenant) específica.
        /// </summary>
        /// <param name="profile">La entidad <see cref="UserProfile"/> que contiene el UserId, CompanyId y estado inicial.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona, devolviendo el <see cref="UserProfile"/> 
        /// persistido con su identificador único generado.
        /// </returns>
        /// <remarks>
        /// Se recomienda validar la existencia previa de la relación Usuario-Empresa antes de invocar este método 
        /// para evitar duplicidad de perfiles en el mismo tenant.
        /// </remarks>
        Task<UserProfile> CreateNewUserProfile(UserProfile profile, CancellationToken cancellationToken);
    }
}