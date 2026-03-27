using ERP.Core.Manager.Api.Domain.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll
{
    /// <summary>
    /// Define el contrato para la gestión y persistencia de la información laboral de los empleados 
    /// dentro del módulo de Nómina (datos contractuales, salarios y posiciones).
    /// </summary>
    public interface IWorkingInformationRepository : IRepository<WorkingInformation>
    {
        /// <summary>
        /// Registra la información laboral asociada a un empleado en el sistema.
        /// </summary>
        /// <param name="workingInformation">Entidad que contiene los detalles del contrato, cargo y condiciones laborales.</param>
        /// <param name="cancellationToken">Token para monitorear y solicitar la cancelación de la operación asíncrona.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona. 
        /// El resultado contiene la entidad <see cref="WorkingInformation"/> con los datos persistidos y sus identificadores.
        /// </returns>
        /// <remarks>
        /// Este método asegura que los datos de vinculación laboral queden vinculados correctamente al perfil del colaborador.
        /// </remarks>
        Task<WorkingInformation> RegisterWorkingInformation(WorkingInformation workingInformation, CancellationToken cancellationToken);
    }
}