using ERP.Core.Database.Domain.Entities.Payrolls;
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

        public decimal Bonus { get; set; }
        public decimal Overtime { get; set; }
        public int NumberOfOvertime { get; set; }
        public decimal GrossSalary { get; set; }

        public decimal Ir { get; set; }
        public decimal Inss { get; set; }
        public decimal TotalLegalDeductions { get; set; }

        public string DeductionsAdditionalData { get; set; } = "{}";
        public decimal TotalDeducctions { get; set; }


        public decimal TravelExpenses { get; set; }
        public decimal FoodTravelAllowance { get ; set; }
        public decimal Lodging { get; set; }
        public decimal TotalTravelExpenses { get; set; }


        public decimal Vacations { get; set; }
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
        public DateTime EntryDate { get; set; }
    }
}