using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface ICalculatorDeductions
    {
        decimal CalculateAntique(decimal monthlySalary, DateTime payrollStartDate, DateTime collaboratorEntryDate);
        Task<decimal> CalculateInss(decimal GrossSalary, CancellationToken cancellationToken);
        Task<IrCalculationResult>CalculateIr(int NFortnight, decimal AccumulatedAccrued,decimal AccumulatedIR, decimal GrossSalary, CancellationToken cancellationToken);

        Task RegisterOrdinaryPayrollForCollaborator(Guid payrollId, Collaborator collaborator, CancellationToken cancellationToken);
    }
}