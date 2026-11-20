using System.Text.Json.Serialization;
using ERP.Core.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Terminations.v1.Commands
{
   public class CloseTerminationCommand : BaseRequest, IRequest<bool>
   {
      [JsonIgnore]
      public Guid TerminationId { get; set; }
   }
}
