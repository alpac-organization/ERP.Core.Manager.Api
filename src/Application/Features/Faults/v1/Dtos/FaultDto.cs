using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos
{
   public class FaultDto
   {
      public Guid FaultId { get; set; }
      public Guid CollaboratorId { get; set; }
      public string CollaboratorName { get; set; } = string.Empty;
      public FaultCatalogReference FaultCatalog { get; set; } = new();
      public DateOnly OccurrenceDate { get; set; }
      public FaultSeverity Severity { get; set; }
      public FaultSource Source { get; set; }
      public FaultStatus Status { get; set; }
   }

   public class FaultCatalogReference
   {
      public Guid FaultCatalogId { get; set; }
      public string Code { get; set; } = string.Empty;
      public string Name { get; set; } = string.Empty;
   }
}