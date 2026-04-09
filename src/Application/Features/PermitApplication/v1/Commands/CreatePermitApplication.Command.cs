using MediatR;
using ERP.Core.Database.Domain.Enums;
using System.Text.Json.Serialization;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands
{
    public class CreatePermitApplicationCommand : BaseRequest, IRequest<bool>
    {
        public string? Description { get; set; }
        public PermitApplicationType PermitApplicationType { get; set; }

        
        public PermitApplicationVacation? PermitApplicationVacation { get; set; }
        public PermitApplicationDonatedVacations? PermitApplicationDonatedVacations { get; set; }
        public PermitApplicationMedicalAppointment? PermitApplicationMedicalAppointment { get; set; }


        [JsonIgnore]
        public string? IdentificationNumber { get; set; }
    }

    public class PermitApplicationVacation
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        public bool IsFullDay { get; set; } = false;
        public bool WithRangeHours { get; set; } = false;
        public bool IsItMidday { get; set; } = false;
    }

    public class PermitApplicationMedicalAppointment
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
    }

    public class PermitApplicationDonatedVacations
    {
        public decimal AmountDays { get; set; }
        public string? IdentificationCollaboratorToReceive { get; set; }
    }
}