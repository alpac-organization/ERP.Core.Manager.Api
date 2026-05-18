using ERP.Core.Database.Domain.Entities.Payrolls;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IIncomeServices
    {
        Task ApplyIncomeOvertime(Collaborator collaboratorInformation, Salary salaryInformation, decimal totalHours, Guid payrollId, Guid incomeTypeId);
        Task ApplyIncomeCommissions(Collaborator collaboratorInformation, Salary salaryInformation, decimal amountComission, Guid payrollId, Guid incomeTypeId);

    }
}