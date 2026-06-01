using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Commands
{
    public class DeleteCostCenterCommand : BaseRequest, IRequest<bool>
    {
        public Guid AreaId { get; set; }
        public Guid CostCenterId { get; set; }
    }
}