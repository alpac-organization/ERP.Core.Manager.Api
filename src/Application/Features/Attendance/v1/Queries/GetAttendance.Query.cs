using ERP.Core.Manager.Api.Application.Features.Attendance.v1.Dtos;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Attendance.v1.Queries
{
    public class GetAttendanceQuery : IRequest<List<AttendanceDto>>
    {
        public int PagseSize { get; set; }
        public int PageNumber { get; set; }
    }
}