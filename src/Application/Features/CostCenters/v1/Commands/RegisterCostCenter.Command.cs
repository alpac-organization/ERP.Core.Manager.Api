using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Commands
{
    public class RegisterCostCenterCommand : BaseRequest, IRequest
    {
        public Guid AreaId { get; set; }
        public int CoilCode { get; set; }
        public string? CostCenterName { get; set; }
        public string? Description { get; set; }
    }
}