using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Queries
{
    public class CancelPermitRequestQuery : BaseRequest, IRequest<bool>
    {
        public Guid PermitApplicationRequestId { get; set; }
    }
}
