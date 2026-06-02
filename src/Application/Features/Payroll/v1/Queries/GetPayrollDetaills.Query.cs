using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries
{
    public class GePayrollDetaillsQuery: BaseRequest, IRequest<PayrollDetailsDto>
    {
        public Guid BranchId { get; set; }
        public Guid PayrollId { get; set; }
        public PayrollType Type { get; set; }

        public int? WorkAreaId { get; set; }
        public int? WorkPositionId { get; set; }
        public string? IdentificationNumber { get; set; }
        
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}