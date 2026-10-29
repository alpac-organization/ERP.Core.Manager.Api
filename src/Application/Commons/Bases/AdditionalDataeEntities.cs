using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Commons.Bases
{
    public class AdditionalDataPermitApplication
    {
        public MedicalAppointmentData MedicalAppointmentData { get; set; } = new();

        public LeaveEvidenceData LeaveEvidenceData {get; set;} = new();
    }

    public class MedicalAppointmentData
    {
        public bool IsFullDay { get; set; }
        public List<ImageData> ImagesAttached { get; set; } = [];   
    }

    public class LeaveEvidenceData
    {
        public bool IsPaid {get; set;}

        public List<ImageData> Evidence {get; set;} = [];

        public int RequireApprovalSteps {get; set;}

        public bool ConsumesVacationBalance {get; set;}
    }
}