using ERP.Core.Manager.Api.Domain.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll
{
    /// <summary>
    /// Define las operaciones de persistencia para la entidad de Colaboradores 
    /// dentro del módulo de Nómina (Payroll).
    /// </summary>
    public interface ICollaboratorsRepository : IRepository<Collaborator>
    {
        /// <summary>
        /// Registra un nuevo colaborador en el sistema, incluyendo su información 
        /// inicial de contrato y datos personales.
        /// </summary>
        /// <param name="collaborator">La entidad <see cref="Collaborator"/> con los datos a persistir.</param>
        /// <returns>
        /// Una tarea que representa la operación asincrónica. 
        /// El resultado de la tarea contiene el colaborador registrado con su ID generado (<see cref="Guid"/>).
        /// </returns>
        /// <exception cref="Exception">Se lanza si ocurre un error durante la persistencia en PostgreSQL.</exception>
        Task<Collaborator> RegisterCollaborator(Collaborator collaborator, CancellationToken cancellationToken);
    }
}