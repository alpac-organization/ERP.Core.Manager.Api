using System.Text.Json.Serialization;
using ERP.Core.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Commands
{
   public class CancelFaultCommand : BaseRequest, IRequest<Unit>
   {
      [JsonIgnore]
      public Guid FaultId { get; set; }
   }
}
