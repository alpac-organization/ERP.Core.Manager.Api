using ERP.Core.Manager.Api.Domain.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll
{
    public interface IVacationRequestsRepository : IRepository<VacationRequest>
    {
        Task<VacationRequest> CreateVacationRequest(VacationRequest payload);
    }
}