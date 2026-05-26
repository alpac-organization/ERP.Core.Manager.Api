using ERP.Core.Database.Domain.Entities.Payrolls;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IDeductionsServices
    {
        Task ApplyDeductionTravelExpenses(Collaborator collaboratorInformation, Salary salaryInformation, Guid PayrollId);
        Task ApplyDeductionLateArrivals(Collaborator collaboratorInformation, Salary salaryInformation, decimal totalMinutes, Guid payrollId);
        Task ApplyDeductionPurisima(Collaborator collaboratorInformation, decimal fortnightlyAmount, Guid payrollId);
    }
}