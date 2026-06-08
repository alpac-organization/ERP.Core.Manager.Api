using ERP.Core.Database.Domain.Entities.Payrolls;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IReportingServices
    {
        Task ApplyVacationMovement(Collaborator collaborator, Guid payrollId);
    }
}