using System.Text.Json.Serialization;
using ERP.Core.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Commands
{
   public class ProcessSanctionCommand : BaseRequest, IRequest<bool>
   {
      [JsonIgnore]
      public Guid SanctionId { get; set; }      

      /// <summary>
      /// Nómina del periodo al aplicar una suspensión.
      /// </summary>
      public Guid? PayrollId { get; set; }

      public bool IsApproved { get; set; }

      /// <summary>
      /// Obligatorio cuando IsApproved es false (rechazo de sanción Pending).
      /// </summary>
      public string? CancellationReason { get; set; }
   }
}
