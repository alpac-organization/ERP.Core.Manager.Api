using MediatR;
using System.Text.Json.Serialization;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Queries
{
    public class GetCostCentersByAreaQuery : BaseRequest, IRequest<List<CostCenterDto>>
    {
        [JsonIgnore]
        public Guid AreaId { get; set; }
    }
}