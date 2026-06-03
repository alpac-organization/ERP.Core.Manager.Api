using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Dtos
{
    public class PermitApplicationDto
    {
        public Guid PayrollId { get; set; }
        public Guid CollaboratorId { get; set; }
        public Guid PermitApllicationId { get; set; }


        public string? Description { get; set; }
        public string? RequestedBy { get; set; }
        public string? AdditionalData { get; set; }
        public string? CollaboratorCode { get; set; }


        //Estados de la solicitud
        public SteptStatus FirstStepStatus { get; set; } = new ();
        public SteptStatus SecondStepStatus { get; set; } = new ();

        public decimal? AmountDays { get; set; }

        
        public TimeOnly? EndTime { get; set; }
        public TimeOnly? StartTime { get; set; }

        public DateOnly EndDate { get; set; }
        public DateOnly StartDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public PermitApplicationStatus Status { get; set; }
        public PermitApplicationType Type { get; set; }
    }

    public class SteptStatus
    {
        public bool IsApproved { get; set; }
        public string? ReviewedBy { get; set; }
    }
}