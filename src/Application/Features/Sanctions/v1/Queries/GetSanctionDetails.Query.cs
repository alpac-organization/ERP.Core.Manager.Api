using System.Text.Json.Serialization;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Dtos;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Queries
{
   public class GetSanctionDetailsQuery : BaseRequest, IRequest<SanctionDetailDto>
   {
      [JsonIgnore]
      public Guid SanctionId { get; set; }
   }
}
