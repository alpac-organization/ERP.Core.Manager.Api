using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Domain.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll
{
    /// <summary>
    /// Define el contrato para la gestión y persistencia de la información laboral de los empleados 
    /// dentro del módulo de Nómina (datos contractuales, salarios y posiciones).
    /// </summary>
    public interface IWorkingInformationRepository : IRepository<WorkingInformation>
    {
        Task<WorkingInformation> RegisterWorkingInformation(WorkingInformation workingInformation);
    }
}