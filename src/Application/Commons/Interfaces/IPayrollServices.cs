using ERP.Core.Database.Domain.Entities.Payrolls;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IPayrollServices
    {
        Task<int> CalculateNumberDaysToAssignedTravelExpenses(Collaborator collaborator, DateOnly payrollStart, DateOnly payrollEnd);
        Task RegisterOrdinaryPayrollForCollaborator(Guid payrollId, Collaborator collaborator, CancellationToken cancellationToken);
    }
}