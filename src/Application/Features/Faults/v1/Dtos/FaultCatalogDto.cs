using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos
{
   public class FaultCatalogDto
   {
      public Guid FaultCatalogId { get; set; }
      public Guid CompanyId { get; set; }
      public string Code { get; set; } = string.Empty;
      public string Name { get; set; } = string.Empty;
      public string Description { get; set; } = string.Empty;
      public bool IsActive { get; set; }
      public FaultSeverity FaultSeverity { get; set; }
   }
}