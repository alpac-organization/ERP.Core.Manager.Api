using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Terminations.v1.Dtos
{
   public class TerminationDto
   {
      public Guid TerminationId { get; set; }
      public Guid CollaboratorId { get; set; }
      public string CollaboratorName { get; set; } = string.Empty;
      public string? IdentificationNumber { get; set; }
      public Guid? SanctionId { get; set; }
      public TerminationType TerminationType { get; set; }
      public TerminationProcessStatus TerminationStatus { get; set; }
      public string Reason { get; set; } = string.Empty;
      public DateOnly DepartureDate { get; set; }
      public DateOnly? NoticeDate { get; set; }
      public int? NoticeDays { get; set; }
      public DateTime CreatedAt { get; set; }
   }
}
