using MediatR;
using AutoMapper;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Attendance.v1.Queries;
using ERP.Core.Clock.Database.Application.Commons.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Application.Features.Attendance.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Attendance.v1.Handlers
{
    public class GetAttendanceHandler(IClockUnitOfWork _clockUnitOfWork, IMapper _mapper) : IRequestHandler<GetAttendanceQuery, List<AttendanceDto>>
    {
        public async Task<List<AttendanceDto>> Handle(GetAttendanceQuery request, CancellationToken cancellationToken)
        {

            // var readings = await _clockUnitOfWork.Readings.Entities
            //     .ToListAsync(cancellationToken);

            return [];
        }
    }
}