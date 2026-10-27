using System.Text.Json.Serialization;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Queries
{
   public class GetFaultDetailsQuery : BaseRequest, IRequest<FaultDetailDto>
   {
      [JsonIgnore]
      public Guid FaultId { get; set; }
   }
}