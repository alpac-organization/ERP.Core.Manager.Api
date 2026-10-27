using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;
using System.Text.Json.Serialization;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Queries
{
   public class GetSuggestedSanctionQuery : BaseRequest, IRequest<SuggestedSanctionDto>
   {
      [JsonIgnore]
      public Guid FaultId { get; set; }      
   }
}