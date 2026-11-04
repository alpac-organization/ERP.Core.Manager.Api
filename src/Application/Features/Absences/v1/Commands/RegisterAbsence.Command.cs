using MediatR;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Absences.v1.Commands
{
    public class RegisterAbsenceCommand : BaseRequest, IRequest<bool>
    {
        public Guid CollaboratorId { get; set; }
        public DateOnly Date { get; set; }
        public string? Reason { get; set; }
        public AbsenceSource Source { get; set; } = AbsenceSource.Manual;
    }
}
