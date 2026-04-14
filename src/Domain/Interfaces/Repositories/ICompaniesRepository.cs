using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Domain.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz de repositorio especializada para la entidad <see cref="Company"/>.
    /// Proporciona métodos específicos de acceso a datos para la gestión de empresas.
    /// </summary>
    public interface ICompaniesRepository : IRepository<Company>
    {
        /// <summary>
        /// Recupera una lista de empresas que cumplen con los criterios de disponibilidad (ej. activas y no eliminadas).
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación para abortar la operación asíncrona si es necesario.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona. 
        /// El resultado contiene una colección de objetos <see cref="Company"/>.
        /// </returns>
        Task<List<Company>> GetAvailableCompanies(CancellationToken cancellationToken);
    }
}