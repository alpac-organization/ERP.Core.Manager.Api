using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos
{
    public class PayrollDto
    {
        public Guid PayrollId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PayrollType Type { get; set; }
        public string? BranchName { get; set; }

        public PaginatedDetailsDto PayrollDetails { get; set; } = new();
    }


    public class PaginatedDetailsDto
    {
        public List<PyrollDtailsDto> Items { get; set; } = [];
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }

    public class PyrollDtailsDto
    {
        public Guid OrdinaryPayrollId { get; set; }
        public decimal BiweeklySalary { get; set; }
        public decimal Ir { get; set; }
        public decimal Inss { get; set; }
        public decimal TotalLegalDeductions { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal TotalToPay { get; set; }

        public CollaboratorInformationDto? Collaborator { get; set; }
    }

    public class CollaboratorInformationDto
    {
        public string? FullName { get; set; }
        public string? CollaboratorCode { get; set; }
        public string? IdentificationNumber { get; set; }
    }
}