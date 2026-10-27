using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos
{
   public class SuggestedSanctionDto
   {
      /// <summary>Falta consultada (puede ser la hija).</summary>
      public Guid FaultId { get; set; }

      /// <summary>Falta sobre la que se debe sancionar (padre si hubo escalamiento).</summary>
      public Guid SanctionTargetFaultId { get; set; }

      public Guid? ParentFaultId { get; set; }
      public bool IsCoveredByEscalation { get; set; }

      public int CountOccurrenceInMonth { get; set; }
      public FaultSeverity Severity { get; set; }
      public SanctionType SuggestedSanctionType { get; set; }
      public List<SanctionType> AllowedSanctionTypes { get; set; } = [];
   }
}
