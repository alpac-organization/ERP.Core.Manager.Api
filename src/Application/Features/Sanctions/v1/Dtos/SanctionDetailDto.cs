using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Helpers;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Dtos
{
   public class SanctionDetailDto : SanctionDto
   {
      public string? IdentificationNumber { get; set; }
      public FaultCatalogReference FaultCatalog { get; set; } = new();
      public DateOnly OccurrenceDate { get; set; }
      public FaultSeverity FaultSeverity { get; set; }
      public Guid? PayrollId { get; set; }
      public Guid IssuedBy { get; set; }
      public RegisterUserDto? IssuedByUser { get; set; }
      public List<EvidenceDto> Evidences { get; set; } = [];
   }
}
