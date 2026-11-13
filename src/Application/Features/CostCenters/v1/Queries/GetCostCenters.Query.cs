using MediatR;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Queries
{
    public class GetCostCentersQuery : BaseRequest, IRequest<List<CostCenterDto>>
    {
        public bool? OnlyOperational { get; set; }
    }
}