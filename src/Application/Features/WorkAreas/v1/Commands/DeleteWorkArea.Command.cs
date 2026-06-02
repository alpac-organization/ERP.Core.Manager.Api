using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Commands
{
    public class DeleteWorkAreaCommand : BaseRequest, IRequest<bool>
    {
        public Guid WorkAreaId { get; set; }
    }
}
