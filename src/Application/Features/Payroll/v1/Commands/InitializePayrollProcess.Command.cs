using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Commands
{
    public class InitializePayrollProcessCommand: BaseRequest, IRequest<bool>
    {
        public PayrollType Type { get; set; }
    }
}