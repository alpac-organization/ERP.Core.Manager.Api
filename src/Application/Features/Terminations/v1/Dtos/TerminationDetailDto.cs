using ERP.Core.Manager.Api.Application.Commons.Helpers;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Terminations.v1.Dtos
{
   public class TerminationDetailDto : TerminationDto
   {
      public string? CancellationReason { get; set; }
      public Guid RegisteredBy { get; set; }
      public DisciplinaryInformationUserDto? RegisteredByUser { get; set; }
      public Guid? ClosedBy { get; set; }
      public DisciplinaryInformationUserDto? ClosedByUser { get; set; }
      public DateTime? ClosedAt { get; set; }
      public List<EvidenceDto> Evidences { get; set; } = [];
   }
}
