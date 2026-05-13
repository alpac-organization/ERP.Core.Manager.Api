using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface ICalculatorDeductions
    {
        Task<decimal> CalculateInss(decimal GrossSalary, CancellationToken cancellationToken);
        Task<decimal> CalculateIr(decimal monthlySalary, int daysWorked, CancellationToken cancellationToken);

        Task<IrCalculationResult>CalculateIrToNextProcess(int NFortnight, decimal AccumulatedAccrued,decimal AccumulatedIR, decimal GrossSalary, CancellationToken cancellationToken);

        Task RegisterOrdinaryPayrollForCollaborator(Guid payrollId, Collaborator collaborator, CancellationToken cancellationToken);
    }
}