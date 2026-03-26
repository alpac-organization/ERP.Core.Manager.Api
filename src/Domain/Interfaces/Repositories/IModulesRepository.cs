using ERP.Core.Manager.Api.Domain.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Catalogs;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Define el contrato de persistencia para la gestión de módulos asociados a empresas.
    /// </summary>
    public interface IModulesRepository : IRepository<Module>
    {
        /// <summary>
        /// Recupera una colección de módulos activos vinculados a una empresa específica.
        /// </summary>
        /// <param name="CompanyId">Identificador único de la empresa.</param>
        /// <param name="cancellationToken">Token para monitorear solicitudes de cancelación de la operación.</param>
        /// <returns>Una lista de entidades <see cref="Module"/> filtradas por estado activo y empresa.</returns>
        Task<List<Module>> ObtainActiveModulesByCompanyId(Guid CompanyId, CancellationToken cancellationToken);

        /// <summary>
        /// Registra un nuevo módulo en el sistema. 
        /// El identificador del módulo será generado automáticamente por la base de datos al persistir.
        /// </summary>
        /// <param name="Payload">Entidad que contiene los datos del módulo a crear.</param>
        /// <param name="cancellationToken">Token para monitorear solicitudes de cancelación de la operación.</param>
        /// <returns>Una tarea que representa la operación de guardado asíncrono.</returns>
        Task CreateModuleAssociatedWithCompany(Module Payload, CancellationToken cancellationToken); 
    }
}