using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IIncomeServices
    {
        Task<bool> ApplyMedicalSubsidyToPregnantWomen(Collaborator collaborator, Payroll period, Salary salary, RegisterSubsidyCommmand subsidyCommmand);

        Task<bool> ApplyMedicalSubsidy(Collaborator collaboratorInformation, Salary salaryInformation, Payroll period, RegisterSubsidyCommmand data);

        Task<bool> ApplyVacationPay(Collaborator collaboratorInformation, Salary salaryInformation, Guid payrollId, decimal amountDays);

        Task ApplyIncomeOvertime(Collaborator collaboratorInformation, Salary salaryInformation, decimal totalHours, Guid payrollId, Guid incomeTypeId);

        Task ApplyIncomeBonus(Collaborator collaboratorInformation, Salary salaryInformation, decimal amountBonus, Currency currency, Guid payrollId, Guid incomeTypeId);

        Task ApplyIncomeCommissions(Collaborator collaboratorInformation, Salary salaryInformation, decimal amountComission, Currency currency, Guid payrollId, Guid incomeTypeId);

        Task ApplyIncomeDepreciation(Collaborator collaboratorInformation, Salary salaryInformation, decimal amountDepreciation, Currency currency, Guid payrollId, Guid incomeTypeId);
    }
}