namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos
{
    public class CheckPayrollDto
    {
        public Guid? PayrollId { get; set;}
        public bool ExistPayrollInProgress { get; set; }
    }
}