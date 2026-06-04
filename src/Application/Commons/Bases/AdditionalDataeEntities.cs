using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Commons.Bases
{
    public class AdditionalDataPermitApplication
    {
        public MedicalAppointmentData MedicalAppointmentData { get; set; } = new();
    }

    public class MedicalAppointmentData
    {
        public bool IsFullDay { get; set; }
        public List<ImageData> ImagesAttached { get; set; } = [];   
    }
}