using ERP.Core.Manager.Api.Application.Features.Attendance.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Attendance.v1.Queries
{
    public class GetAttendanceQuery : IRequest<PagedResponse<AttendanceDto>>
    {
        public DateTime StartDate { get; set;}
        public Guid CompanyId { get; set; }
        public DateTime EndDate { get; set; }
        public string? IdentificationNumber { get; set; }

        public Guid? BranchId { get; set; }
        
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}