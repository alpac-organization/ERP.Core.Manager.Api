using ERP.Core.Database.Domain.Entities.Payroll;
using ERP.Core.Manager.Api.Domain.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll
{
    /// <summary>
    /// Define el contrato para la persistencia y gestión de la información personal de los empleados.
    /// </summary>
    public interface IPersonalInformationRepository : IRepository<PersonalInformation>
    {
        /// <summary>
        /// Registra una nueva entidad de información personal en el sistema de nómina.
        /// </summary>
        /// <param name="personalInformation">La entidad con los datos personales a persistir.</param>
        /// <param name="cancellationToken">Token de cancelación para abortar la operación asíncrona si es necesario.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona. 
        /// El resultado de la tarea contiene la entidad <see cref="PersonalInformation"/> procesada (incluyendo IDs generados).
        /// </returns>
        Task<PersonalInformation> RegisterPersonalInformation(PersonalInformation personalInformation, CancellationToken cancellationToken);
    }
}