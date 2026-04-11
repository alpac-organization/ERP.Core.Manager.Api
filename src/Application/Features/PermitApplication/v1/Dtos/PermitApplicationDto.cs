using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Dtos
{
    public class PermitApplicationDto
    {
        public Guid PermitApllicationId { get; set; }
        public Guid CollaboratorId { get; set; }
        public string? CollaboratorCode { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public string? Description { get; set; }
        public string? RequestedBy { get; set; }

        
        //Nombres de los aprobadores
        public string? ManagerFullname { get; set; }
        public string? AdministratorFullName { get; set; }

        public bool? FirtsStepApproved { get; set; } = null;
        public bool? SecondStepApproved { get; set; } = null; 

        public DateTime CreatedAt { get; set; }

        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        public PermitApplicationStatus Status { get; set; }
        public PermitApplicationType Type { get; set; }
    }
}