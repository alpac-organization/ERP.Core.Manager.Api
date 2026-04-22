using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;
namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries
{
    public class CheckIfThereIsPayrollInProgressQuery: BaseRequest, IRequest<CheckPayrollDto>
    {
        public Guid BranchId { get; set; }
        public PayrollType PayrollType { get; set; }
    }
}
