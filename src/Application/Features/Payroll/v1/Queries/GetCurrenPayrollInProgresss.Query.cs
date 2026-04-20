using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries
{
    public class GetCurrenPayrollInProgresssQuery: BaseRequest, IRequest<PayrollDto>
    {
        public PayrollType Type { get; set; }

        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}