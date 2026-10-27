using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Commands
{
   public class RegisterSanctionCommand : BaseRequest, IRequest<bool>
   {
      public Guid FaultId { get; set; }
      public SanctionType SanctionType { get; set; }
      public string Reason { get; set; } = string.Empty;
      public DateOnly? SuspensionStartDate { get; set; }
      public DateOnly? SuspensionEndDate { get; set; }
      public List<Evidence> Evidences { get; set; } = [];
   }

   public class Evidence
   {
      public string? FileName { get; set; }
      public string? ImageBase64 { get; set; }
      public string? ContentType { get; set; }
   }
}
