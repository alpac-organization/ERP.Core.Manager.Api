using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Reports.v1.Dtos;
namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IReportingServices
    {
        Task RegisterChrismasBonus(Collaborator collaborator, Guid payrollId);

        Task ApplyVacationMovement(Collaborator collaborator, Guid payrollId);

        Task ApplyVacationRegistration(Collaborator collaborator, Guid payrollId);

        Task ApplyInssReporting(string period, Guid payrollId, Collaborator collaborator, decimal biweeklySalary, decimal inssLabor, decimal? patronalInatecBase = null);

        Task<bool> ApplyUpdateIrReporting(Collaborator collaborator, decimal newIR, decimal newSalaryEarned, Payroll payroll, CancellationToken cancellationToken = default);

        Task<IrAndSalaryEarnedReport> ApplyIrReporting(Payroll payroll, Guid collaboratorId, decimal irFortnightly, decimal salaryEarnedFortnightly,
        CancellationToken cancellationToken = default);

        Task<List<IrAndSalaryEarnedReport>> GetIrAndSalaryEarnedReport(Guid payrollId, Guid companyId, PayrollType payrollType, string? identificationNumber, Guid? areaId, CancellationToken cancellationToken = default);
    }
}