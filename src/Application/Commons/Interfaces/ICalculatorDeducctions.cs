using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface ICalculatorDeductions
    {
        Task<decimal> CalculateInss(decimal GrossSalary, CancellationToken cancellationToken);
        Task<IrCalculationResult>CalculateIr(int NFortnight, decimal AccumulatedAccrued,decimal AccumulatedIR, decimal GrossSalary, CancellationToken cancellationToken, bool isSubsidy = false);

        Task RegisterOrdinaryPayrollForCollaborator(Guid payrollId, Collaborator collaborator, CancellationToken cancellationToken);
        decimal CalculateAntique(decimal monthlySalary, DateOnly payrollStartDate, DateOnly collaboratorEntryDate);
    }
}