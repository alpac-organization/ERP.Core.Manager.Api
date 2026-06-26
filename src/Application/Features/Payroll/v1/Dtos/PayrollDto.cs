using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos
{
    public class PayrollDto
    {
        public Guid PayrollId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
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

        public decimal Bonus { get; set; }
        public decimal Overtime { get; set; }
        public decimal Commissions { get; set; }
        public decimal Antique { get; set; }
        public decimal NumberOvertime { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal TotalIncome { get; set; }

        public decimal Ir { get; set; }
        public decimal Inss { get; set; }
        public decimal TotalLegalDeductions { get; set; }

        public string DeductionsAdditionalData { get; set; } = "{}";
        public decimal TotalDeducctions { get; set; }


        public decimal Transport { get; set; }
        public decimal Feeding { get ; set; }
        public decimal Lodging { get; set; }
        public decimal TotalTravelExpenses { get; set; }


        public decimal Vacations { get; set; }
        public decimal AmountDaysVacation { get; set; }
        public decimal TotalToPay { get; set; }


        public CollaboratorInformationDto? Collaborator { get; set; }
    }

    public class CollaboratorInformationDto
    {
        public string? FullName { get; set; }
        public string? InssNumber { get; set; }
        public string? CollaboratorCode { get; set; }
        public string? JobPosition { get; set; }
        public string? WorkArea { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? BankAccount { get; set; }
        public DateOnly EntryDate { get; set; }
    }
}