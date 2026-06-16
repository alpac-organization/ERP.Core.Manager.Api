using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Attendance.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Attendance.v1.Queries;
using ERP.Core.Clock.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Attendance.v1.Handlers
{
    public class GetAttendanceHandler( IClockUnitOfWork _clockUnitOfWork, IMapper _mapper) : IRequestHandler<GetAttendanceQuery, PagedResponse<AttendanceDto>>
    {
        public async Task<PagedResponse<AttendanceDto>> Handle(GetAttendanceQuery request, CancellationToken cancellationToken)
        {
            var startDate = request.StartDate.Date;
            var endDate = request.EndDate.Date.AddDays(1);

            var query = _clockUnitOfWork.Readings.Entities
                .Include(x => x.Employee)
                .AsNoTracking()
                .Where(x => x.ReadTime >= startDate &&
                            x.ReadTime < endDate);

            if (!string.IsNullOrWhiteSpace(request.IdentificationNumber))
            {
                query = query.Where(x =>
                    x.Employee.ErpCollaboratorId == request.IdentificationNumber);
            }

            var totalItems = await query.CountAsync(cancellationToken);

            var readings = await query
                .OrderByDescending(x => x.ReadTime)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var items = _mapper.Map<List<AttendanceDto>>(readings);

            return new PagedResponse<AttendanceDto>(
                items,
                request.PageNumber,
                request.PageSize,
                totalItems
            );
        }
    }
}