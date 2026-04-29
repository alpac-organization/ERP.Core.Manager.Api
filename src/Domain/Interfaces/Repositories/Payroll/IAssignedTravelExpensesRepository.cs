using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Domain.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll
{
    public interface IAssignedTravelExpensesRepository: IRepository<AssignedTravelExpenses>
    {
        Task<AssignedTravelExpenses> RegisterAssignedTravelExpenses(AssignedTravelExpenses assignedTravelExpenses); 
    }
}