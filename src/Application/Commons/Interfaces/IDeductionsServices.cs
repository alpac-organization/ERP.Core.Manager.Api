using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IDeductionsServices
    {
        Task ApplyDeductionTravelExpenses(Collaborator collaboratorInformation, Salary salaryInformation, Guid PayrollId);
        Task ApplyDeductionLateArrivals(Collaborator collaboratorInformation, Salary salaryInformation, decimal totalMinutes, Guid payrollId);
        Task ApplyDeductionPurisima(Collaborator collaboratorInformation, decimal fortnightlyAmount, Guid payrollId, int numberFortnights);
        Task ApplyDeductionLoans(Collaborator collaboratorInformation, decimal amount, Guid payrollId, int numberFortnights, Currency currency, string description = "Registro de préstamo");
        Task ApplyJudicialGarnishment(Collaborator collaborator, decimal totalAmount, int percentage, Currency currency, string description, Guid payrollId);
        Task<bool> ApplySansion(Collaborator collaboratorInformation, int amountDays, Guid payrollId);
    }
}