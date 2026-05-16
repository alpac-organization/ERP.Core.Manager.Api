using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Commands
{
    public class ClosePayrollProcessCommand: BaseRequest, IRequest<bool>
    {
        public Guid PayrollId { get; set; }
        public Guid BranchId { get; set; }
        public PayrollType PayrollType { get; set; }
    }
}