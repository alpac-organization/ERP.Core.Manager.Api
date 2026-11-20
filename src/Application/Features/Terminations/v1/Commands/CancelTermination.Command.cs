using System.Text.Json.Serialization;
using ERP.Core.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Terminations.v1.Commands
{
   public class CancelTerminationCommand : BaseRequest, IRequest<bool>
   {
      [JsonIgnore]
      public Guid TerminationId { get; set; }

      public string CancellationReason { get; set; } = string.Empty;
   }
}
