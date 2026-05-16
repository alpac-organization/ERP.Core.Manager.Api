namespace ERP.Core.Manager.Api.Application.Commons.Bases
{
    public class AdditionalDataPermitApplication
    {
        public MedicalAppointmentData MedicalAppointmentData { get; set; } = new();
    }

    public class MedicalAppointmentData
    {
        public bool IsFullDay { get; set; }
        
    }

}