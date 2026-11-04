using MediatR;
using ERP.Core.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Absences.v1.Commands
{
    public class CancelAbsenceCommand : BaseRequest, IRequest<bool>
    {
        public Guid AbsenceId { get; set; }
        public string CancellationReason { get; set; } = string.Empty;
    }
}
