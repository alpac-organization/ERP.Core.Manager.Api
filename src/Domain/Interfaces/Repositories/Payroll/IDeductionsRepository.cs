using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Domain.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll
{
    public interface IDeductionsRepository : IRepository<Deduction>
    {
        Task<Deduction> RegisterDeduction(Deduction deduction); 
    }
}