using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Dtos
{
   public class SanctionDetailDto
   {
      public Guid SanctionId { get; set; }
      public Guid FaultId { get; set; }

      public Guid CollaboratorId { get; set; }
      public string CollaboratorName { get; set; } = string.Empty;
      public string? IdentificationNumber { get; set; }

      public FaultCatalogReference FaultCatalog { get; set; } = new();
      public DateOnly OccurrenceDate { get; set; }
      public FaultSeverity FaultSeverity { get; set; }

      public SanctionType SanctionType { get; set; }
      public SanctionStatus SanctionStatus { get; set; }
      public string Reason { get; set; } = string.Empty;

      public DateOnly? SuspensionStartDate { get; set; }
      public DateOnly? SuspensionEndDate { get; set; }

      public Guid? PayrollId { get; set; }
      public Guid IssuedBy { get; set; }
      public RegisterUserDto? IssuedByUser { get; set; }

      public List<EvidenceDto> Evidences { get; set; } = [];
      public DateTime CreatedAt { get; set; }
   }
}
