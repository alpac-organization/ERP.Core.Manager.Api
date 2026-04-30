using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos
{
    public class PayrollPeriodDto
    {
        public Guid PayrollId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PayrollType Type { get; set; }
        public string? BranchId { get; set; }
        public string? BranchName { get; set; }
    }
}