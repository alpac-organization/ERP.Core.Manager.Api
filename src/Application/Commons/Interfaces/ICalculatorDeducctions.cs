namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface ICalculatorDeductions
    {
        public decimal CalculateInss(decimal baseSalary);
        public decimal CalculateIr(decimal monthlySalary);
        public Task RegisterOrdinaryPayrollForCollaborator(Guid PayrollId, Guid CollaboratorId, CancellationToken cancellationToken);
    }
}