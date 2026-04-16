using ERP.Core.Manager.Api.Domain.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll
{
    public interface IPayrollsRepository : IRepository<Database.Domain.Entities.Payrolls.Payroll>
    {
        Task<Database.Domain.Entities.Payrolls.Payroll> InitializePayroll(Database.Domain.Entities.Payrolls.Payroll payload);
    }
}