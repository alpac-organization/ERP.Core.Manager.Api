using ERP.Core.Manager.Api.Application.Features.Branches.v1.Dtos;
using ERP.Core.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Branches.v1.Queries
{
    public class GetBranchesAvailableQuery: BaseRequest, IRequest<List<BranchesDto>>
    {
        public bool? HasWarehouse { get; set; }
    }
}