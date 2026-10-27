using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Dtos
{
   public class SanctionDto
   {
      public Guid SanctionId { get; set; }
      public Guid FaultId { get; set; }
      public Guid CollaboratorId { get; set; }
      public string CollaboratorName { get; set; } = string.Empty;
      public SanctionType SanctionType { get; set; }
      public SanctionStatus SanctionStatus { get; set; }
      public string Reason { get; set; } = string.Empty;
      public DateOnly? SuspensionStartDate { get; set; }
      public DateOnly? SuspensionEndDate { get; set; }
      public DateTime CreatedAt { get; set; }
   }
}
