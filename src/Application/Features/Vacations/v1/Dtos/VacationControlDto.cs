using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos
{
    public class VacationControlDto
    {
        public Guid PermitApplicationId { get; set; }
        public decimal AmountDays { get; set; }
        public string? Description { get; set; }
        public string? WorkPosition { get; set; }
        public string? CollaboratorCode { get; set; }
        public string? CollaboratorFullname { get; set;  }
        public string? ApprovedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        public string? IdentificationCollaboratorToReceive { get; set; }
        public PermitApplicationType PermitApplicationType { get; set; }
    }
}