using ERP.Core.Database.Domain.Entities.Payrolls;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IReportingServices
    {
        Task RegisterChrismasBonus(Collaborator collaborator, Guid payrollId);

        Task ApplyVacationMovement(Collaborator collaborator, Guid payrollId);

        Task ApplyVacationRegistration(Collaborator collaborator, Guid payrollId);
    }
}