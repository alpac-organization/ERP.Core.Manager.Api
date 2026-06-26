using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos
{
    public class PayrollDetailsDto
    {
        public Guid PayrollId { get; set; }
        public PayrollType Type { get; set; }
        public DateOnly EndDate { get; set; }
        public DateOnly StartDate { get; set; }
        public string? BranchName { get; set; }

        //Listado de información de nomina
        public List<OrdinaryPayrollDetailsDto> OrdinaryPayrollData { get; set; } = [];
        public List<ProfessionalServicePayrollDto> ProfessionalServicesPayrollData { get; set; } = [];

        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }

    public class OrdinaryPayrollDetailsDto : BasePayroll
    {
        public Guid OrdinaryPayrollId { get; set; }

        public decimal Bonus { get; set; }
        public decimal Antique { get; set; }
        public decimal Overtime { get; set; }
        public decimal Commissions { get; set; }
        public decimal NumberOvertime { get; set; }


        public decimal Lodging { get; set; }
        public decimal Feeding { get ; set; }
        public decimal Transport { get; set; }
        public decimal TotalTravelExpenses { get; set; }
    }

    public class ProfessionalServicePayrollDto : BasePayroll
    {
        public Guid ProfessionalServicePayrollId { get; set; }
        
    }


    public class BasePayroll
    {
        public decimal GrossSalary { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal BiweeklySalary { get; set; }

        public decimal Ir { get; set; }
        public decimal Inss { get; set; }
        public decimal Inatec { get; set; }
        public decimal InssPatronal { get; set; }
        public decimal TotalLegalDeductions { get; set; }

        public string DeductionsAdditionalData { get; set; } = "{}";
        public decimal TotalDeducctions { get; set; }


        public decimal Vacations { get; set; }
        public decimal AmountDaysVacation { get; set; }
        public decimal Aguinaldo { get; set; }

        public decimal TotalToPay { get; set; }

        public CollaboratorInformationDto CollaboratorInformation { get; set; } = new();
    }
}