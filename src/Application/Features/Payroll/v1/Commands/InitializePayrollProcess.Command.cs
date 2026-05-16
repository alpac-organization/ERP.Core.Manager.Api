using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Commands
{
    public class InitializePayrollProcessCommand: BaseRequest, IRequest<bool>
    {
        public PayrollType Type { get; set; }
        public Guid BranchId { get; set; }
    }
}