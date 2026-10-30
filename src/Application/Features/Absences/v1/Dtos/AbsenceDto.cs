using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Absences.v1.Dtos
{
    public class AbsenceDto
    {
        public Guid AbsenceId { get; set; }
        public Guid CollaboratorId { get; set; }
        public string Fullname { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty;
        public string? BranchName { get; set; }
        public DateOnly Date { get; set; }
        public AbsenceStatus Status { get; set; }
        public AbsenceSource Source { get; set; }
    }
}
