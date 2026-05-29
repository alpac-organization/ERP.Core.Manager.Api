using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos
{
    public class PayrollPeriodDto
    {
        public Guid PayrollId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public PayrollType Type { get; set; }
        public string? BranchId { get; set; }
        public string? BranchName { get; set; }
    }
}