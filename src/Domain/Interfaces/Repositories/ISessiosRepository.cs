using ERP.Core.Manager.Api.Domain.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Define el contrato para la persistencia y gestión de sesiones de usuario.
    /// Permite el rastreo de accesos, dispositivos y estados de autenticación activa.
    /// </summary>
    public interface ISessionsRepository : IRepository<Session>
    {
        /// <summary>
        /// Registra una nueva sesión de autenticación en el sistema.
        /// </summary>
        /// <param name="session">La entidad <see cref="Session"/> con los detalles del acceso (Token, Dispositivo, IP).</param>
        /// <param name="cancellationToken">Token para la cancelación de la operación asíncrona.</param>
        /// <returns>
        /// Una tarea que representa la operación, devolviendo la <see cref="Session"/> 
        /// persistida con su identificador y fecha de creación.
        /// </returns>
        /// <remarks>
        /// Este método es fundamental para el flujo de login y la validación de concurrencia de usuarios en el ERP.
        /// </remarks>
        Task<Session> CreateNewSession(Session session, CancellationToken cancellationToken);
    }
}