using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Domain.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll
{
    public interface IIncomeTaxAccrualRepository : IRepository<IncomeTaxAccrual>
    {
        Task<IncomeTaxAccrual> RegisterIncomeTaxAccrual(IncomeTaxAccrual incomeTaxAccrual); 
    }
}