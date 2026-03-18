using ERP.Core.Manager.Api.Domain.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Define el contrato para las operaciones de persistencia relacionadas con la entidad <see cref="User"/>.
    /// Extiende de <see cref="IRepository{User}"/> para incluir operaciones base de acceso a datos.
    /// </summary>
    public interface IUsersRepository : IRepository<User>
    {
        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="user">La entidad de usuario con la información a persistir.</param>
        /// <returns>Una tarea que representa la operación asíncrona, devolviendo la entidad <see cref="User"/> creada con su identificador generado.</returns>
        Task<User> CreateNewUser(User user, CancellationToken cancellationToken);

        /// <summary>
        /// Obtiene una colección de todos los usuarios activos asociados a una empresa específica.
        /// </summary>
        /// <param name="company_id">El identificador único de la empresa (Tenant).</param>
        /// <returns>
        /// Una colección <see cref="IEnumerable{User}"/> que contiene los usuarios activos. 
        /// Retorna una lista vacía si no se encuentran registros.
        /// </returns>
        /// <remarks>
        /// Esta consulta aplica un filtro de estado activo (IsActive) y pertenencia jerárquica a la empresa.
        /// </remarks>
        Task<IEnumerable<User>> GetActiveUsersByCompany(int company_id);
    }
}