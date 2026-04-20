using ERP.Core.Database.Domain.Entities.Payrolls;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface ICalculatorDeductions
    {
        Task<decimal> CalculateInss(decimal GrossSalary, CancellationToken cancellationToken);
        Task<decimal> CalculateIr(decimal monthlySalary, int daysWorked, CancellationToken cancellationToken);

        

        //Realizar Operación de calculos de nomina ordinaria e insertar, o crear nuevo ciclo de nomina.
        Task RegisterOrdinaryPayrollForCollaborator(Guid payrollId, Collaborator collaborator, CancellationToken cancellationToken);
    }
}