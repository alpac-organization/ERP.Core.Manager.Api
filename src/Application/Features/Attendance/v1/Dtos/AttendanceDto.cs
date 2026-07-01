namespace ERP.Core.Manager.Api.Application.Features.Attendance.v1.Dtos
{
    public class AttendanceDto 
    {
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? CollaboratorFullname { get; set; }

        public List<MarkingDto> Markings { get; set; } = [];
    }

     public class MarkingDto
    {
        public DateTime ReadTime { get; set; }
        public string? DeviceName { get; set; }
    }
}