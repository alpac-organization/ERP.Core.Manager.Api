using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries
{
    public class GetPermitApplicationInPayrollQuery: BaseRequest, IRequest<List<PermitApplicationDto>>
    {
        public Guid PayrollId { get; set; }

        public string? IdentificationNumber { get; set; }
        public PermitApplicationStatus? Status { get; set; }

        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}