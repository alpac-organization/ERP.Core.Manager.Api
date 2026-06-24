using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IReportingServices
    {
        Task RegisterChrismasBonus(Collaborator collaborator, Guid payrollId);

        Task ApplyVacationMovement(Collaborator collaborator, Guid payrollId);

        Task ApplyVacationRegistration(Collaborator collaborator, Guid payrollId);

        Task ApplyInssReporting(string period, Guid payrollId, Collaborator collaborator, decimal salary);
    }
}