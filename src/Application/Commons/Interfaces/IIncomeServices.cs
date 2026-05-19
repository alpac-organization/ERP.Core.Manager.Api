using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IIncomeServices
    {
        Task ApplyIncomeOvertime(Collaborator collaboratorInformation, Salary salaryInformation, decimal totalHours, Guid payrollId, Guid incomeTypeId);
        Task ApplyIncomeCommissions(Collaborator collaboratorInformation, Salary salaryInformation, decimal amountComission, Currency currency, Guid payrollId, Guid incomeTypeId);

    }
}