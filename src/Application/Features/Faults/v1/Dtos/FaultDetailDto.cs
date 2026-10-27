using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Helpers;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos
{
   public class FaultDetailDto
   {
      public Guid FaultId { get; set; }

      // Colaborador
      public Guid CollaboratorId { get; set; }
      public string CollaboratorName { get; set; } = string.Empty;
      public string? IdentificationNumber { get; set; }

      // Catálogo
      public FaultCatalogReference FaultCatalog { get; set; } = new();

      // Datos de la falta
      public DateOnly OccurrenceDate { get; set; }
      public FaultSeverity Severity { get; set; }
      public FaultSource Source { get; set; }
      public FaultStatus Status { get; set; }

      // Evidencia (jsonb deserializado o string)
      public List<EvidenceDto> Evidences { get; set; } = [];

      // Relación padre (escalamiento)      
      public FaultReference? ParentFault { get; set; }

      // Auditoría
      public RegisterUserDto? RegisteredBy { get; set; }
      public DateTime CreatedAt { get; set; }
   }

   public class FaultReference
   {
      public Guid FaultId { get; set; }
      public DateOnly OccurrenceDate { get; set; }
      public FaultSeverity Severity { get; set; }
      public FaultStatus Status { get; set; }
   }

   public class RegisterUserDto
   {
      public Guid UserId { get; set; }      
      public string? FullName { get; set; }
      public string? Email { get; set; }
      public string? Status { get; set; }
   }

   public class SanctionSummaryDto
   {
      public Guid SanctionId { get; set; }
      public SanctionType SanctionType { get; set; }
      public SanctionStatus SanctionStatus { get; set; }
      public string Reason { get; set; } = string.Empty;
      public DateOnly SuspensionEndDate { get; set; }
      public DateOnly SuspensionStartDate { get; set; }
   }
}