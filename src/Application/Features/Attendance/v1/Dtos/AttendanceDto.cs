namespace ERP.Core.Manager.Api.Application.Features.Attendance.v1.Dtos
{
    public class AttendanceDto 
    {
        public int UserId { get; set; }
        public DateTime ReadTime { get; set; }
        public string? Devicename { get; set; }
        public string? CollaboratorFullname { get; set; }
    }
}