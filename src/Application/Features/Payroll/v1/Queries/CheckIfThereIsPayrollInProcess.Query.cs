using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries
{
    public class CheckIfThereIsPayrollInProgressQuery: BaseRequest, IRequest<CheckPayrollDto>
    {
        public PayrollType PayrollType { get; set; }
    }
}
