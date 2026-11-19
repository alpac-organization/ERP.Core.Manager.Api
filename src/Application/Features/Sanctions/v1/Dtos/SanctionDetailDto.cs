using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Helpers;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Dtos
{
   public class SanctionDetailDto : SanctionDto
   {
      public string? IdentificationNumber { get; set; }
      public FaultCatalogReference FaultCatalog { get; set; } = new();
      public DateOnly OccurrenceDate { get; set; }
      public FaultSeverity FaultSeverity { get; set; }      
      public Guid IssuedBy { get; set; }
      public DisciplinaryInformationUserDto? IssuedByUser { get; set; }
      public Guid? ApprovedBy { get; set; }
      public DisciplinaryInformationUserDto? ApprovedByUser { get; set; }
      public DateTime? ApprovedAt { get; set; }
      public string? CancellationReason { get; set; }
      public SanctionPayrollReferenceDto? Payroll { get; set; }
      public SanctionDeductionReferenceDto? Deduction { get; set; }
      public List<EvidenceDto> Evidences { get; set; } = [];
   }

   public class SanctionPayrollReferenceDto
   {
      public Guid PayrollId { get; set; }
      public DateOnly StartDate { get; set; }
      public DateOnly EndDate { get; set; }
      public PayrollType Type { get; set; }
      public PayrollStatus Status { get; set; }
   }

   public class SanctionDeductionReferenceDto
   {
      public Guid DeductionId { get; set; }
      public DeductionType Type { get; set; }
      public DeductionStatus Status { get; set; }
      public Currency Currency { get; set; }
      public int? SanctionDays { get; set; } // Deduction.Amount (días) casteado a int
      public decimal TotalAmount { get; set; }
      public decimal TotalAmountInDollars { get; set; }
   }
}
