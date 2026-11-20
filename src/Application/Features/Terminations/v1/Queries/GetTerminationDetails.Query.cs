using System.Text.Json.Serialization;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Dtos;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Terminations.v1.Queries
{
   public class GetTerminationDetailsQuery : BaseRequest, IRequest<TerminationDetailDto>
   {
      [JsonIgnore]
      public Guid TerminationId { get; set; }
   }
}
