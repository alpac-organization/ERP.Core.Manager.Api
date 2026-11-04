using MediatR;
using ERP.Core.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Absences.v1.Commands
{
    public class DeleteAbsenceCommand : BaseRequest, IRequest<bool>
    {
        public Guid AbsenceId { get; set; }
    }
}
