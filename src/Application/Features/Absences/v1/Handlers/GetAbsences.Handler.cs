using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Absences.v1.Handlers
{
    public class GetAbsencesRequestHandler(IUnitOfWork _unitOfWork, IMapper _mapper, IErrorManager _errorManager)
        : AlpacBaseHandler<GetAbsencesQuery, PagedResponse<AbsenceDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<PagedResponse<AbsenceDto>> Handle(GetAbsencesQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);
            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            var query = _unitOfWork.Absences.Entities
                .Include(a => a.Collaborator)
                    .ThenInclude(c => c.WorkingInformation)
                        .ThenInclude(w => w.Branch)
                .Where(a => a.Collaborator.CompanyId == request.CompanyId && a.DeletedAt == null)
                .AsNoTracking();

            if (request.BranchId.HasValue)
            {
                query = query.Where(a => a.Collaborator.WorkingInformation.BranchId == request.BranchId);
            }

            if (request.CollaboratorId.HasValue)
            {
                query = query.Where(a => a.CollaboratorId == request.CollaboratorId);
            }

            if (!string.IsNullOrEmpty(request.IdentificationNumber))
            {
                query = query.Where(a => a.Collaborator.IdentificationNumber == request.IdentificationNumber);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(a => a.Status == request.Status);
            }

            if (request.Source.HasValue)
            {
                query = query.Where(a => a.Source == request.Source);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(a => a.Date >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(a => a.Date <= request.EndDate.Value);
            }

            if (request.PayrollId.HasValue)
            {
                query = query.Where(a => a.PayrollId == request.PayrollId);
            }

            var totalRecords = await query.CountAsync(cancellationToken);

            var records = await query
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var mappedRecords = _mapper.Map<List<AbsenceDto>>(records);

            return new PagedResponse<AbsenceDto>(
                mappedRecords,
                request.PageNumber,
                request.PageSize,
                totalRecords
            );
        }
    }
}
