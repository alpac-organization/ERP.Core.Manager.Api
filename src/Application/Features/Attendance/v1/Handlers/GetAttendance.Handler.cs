using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Attendance.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Attendance.v1.Queries;
using ERP.Core.Clock.Database.Application.Commons.Interfaces.Repositories;
using static ERP.Core.Manager.Api.Application.Features.Attendance.v1.Dtos.AttendanceDto;

namespace ERP.Core.Manager.Api.Application.Features.Attendance.v1.Handlers
{
    public class GetAttendanceHandler(IClockUnitOfWork _clockUnitOfWork, IMapper _mapper)
        : IRequestHandler<GetAttendanceQuery, PagedResponse<AttendanceDto>>
    {
        public async Task<PagedResponse<AttendanceDto>> Handle(GetAttendanceQuery request, CancellationToken cancellationToken)
        {
            var startDate = request.StartDate.Date;
            var endDate = request.EndDate.Date.AddDays(1);

            var baseQuery = _clockUnitOfWork.Readings.Entities
                .AsNoTracking()
                .Where(x => x.ReadTime >= startDate && x.ReadTime < endDate);

            if (!string.IsNullOrWhiteSpace(request.IdentificationNumber))
            {
                baseQuery = baseQuery.Where(x => x.Employee.ErpCollaboratorId == request.IdentificationNumber);
            }

            var groupKeysQuery = baseQuery
                .Select(x => new { x.UserId, Day = x.ReadTime.Date })
                .Distinct();

            var totalItems = await groupKeysQuery.CountAsync(cancellationToken);

            var pagedKeys = await groupKeysQuery
                .OrderByDescending(k => k.Day)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            if (pagedKeys.Count == 0)
            {
                return new PagedResponse<AttendanceDto>([], request.PageNumber, request.PageSize, totalItems);
            }

            var userIds = pagedKeys.Select(k => k.UserId).Distinct().ToList();
            var days = pagedKeys.Select(k => k.Day).Distinct().ToList();

            var readings = await baseQuery
                .Include(x => x.Employee)
                .Include(x => x.Device)
                .Where(x => userIds.Contains(x.UserId) && days.Contains(x.ReadTime.Date))
                .OrderBy(x => x.ReadTime)
                .ToListAsync(cancellationToken);

            var items = readings
                .GroupBy(x => new { x.UserId, Day = x.ReadTime.Date })
                .Select(g => new AttendanceDto
                {
                    UserId = g.Key.UserId,
                    Date = g.Key.Day,
                    IdentificationNumber = g.First().Employee.ErpCollaboratorId,
                    CollaboratorFullname = g.First().Employee.Name,
                    Markings = _mapper.Map<List<MarkingDto>>(g.OrderBy(r => r.ReadTime).ToList())
                })
                .OrderByDescending(x => x.Date)
                .ToList();

            return new PagedResponse<AttendanceDto>(
                items,
                request.PageNumber,
                request.PageSize,
                totalItems
            );
        }
    }
}