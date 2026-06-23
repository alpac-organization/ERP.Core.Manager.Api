using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Handlers
{
    public class GetCollaboratorsAvailableHandler(IUnitOfWork _unitOfWork, IMapper _mapper) 
    : IRequestHandler<GetCollaboratorsAvailableQuery, PagedResponse<GetCollaboratorDto>>
    {
        public async Task<PagedResponse<GetCollaboratorDto>> Handle(GetCollaboratorsAvailableQuery request, CancellationToken cancellationToken)
        {
            var baseQuery = _unitOfWork.Collaborators.Entities
                .AsNoTracking()
                .Where(c => c.CompanyId == request.CompanyId);

            var totalCollaborators = await baseQuery.CountAsync(cancellationToken);
            
            var totalActive = await baseQuery.CountAsync(c => c.Status == CollaboratorStatus.Active, cancellationToken);
            var totalOnVacation = await baseQuery.CountAsync(c => c.Status == CollaboratorStatus.Vacation, cancellationToken);
            var totalOnSubsidy = await baseQuery.CountAsync(c => c.Status == CollaboratorStatus.Subsidy, cancellationToken);

            var gridQuery = baseQuery
                .Include(c => c.WorkingInformation)
                    .ThenInclude(w => w.Area)
                .Include(c => c.WorkingInformation)
                    .ThenInclude(w => w.WorkPosition)
                .Include(c => c.WorkingInformation)
                    .ThenInclude(w => w.BranchInfo)
                .Include(c => c.Vacation)
                .AsQueryable();

            if (request.Status.HasValue)
                gridQuery = gridQuery.Where(c => c.Status == request.Status.Value);
            else
                gridQuery = gridQuery.Where(c => c.Status != CollaboratorStatus.Inactive);

            if (!string.IsNullOrEmpty(request.IdentificationNumber))
                gridQuery = gridQuery.Where(c => c.IdentificationNumber == request.IdentificationNumber);

            if (!string.IsNullOrEmpty(request.BranchId.ToString()))
                gridQuery = gridQuery.Where(c => c.WorkingInformation.CompanyBranchId == request.BranchId);

            if (!string.IsNullOrEmpty(request.AreaId.ToString()))
                gridQuery = gridQuery.Where(c => c.WorkingInformation.AreaId == request.AreaId);

            var filteredRecordsCount = await gridQuery.CountAsync(cancellationToken);

            var collaborators = await gridQuery
                .OrderBy(c => c.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<GetCollaboratorDto>>(collaborators) ?? [];

            return new PagedResponse<GetCollaboratorDto>(
                dtos, 
                filteredRecordsCount, 
                request.PageNumber, 
                request.PageSize,
                totalActive,
                totalOnVacation,
                totalOnSubsidy,
                totalCollaborators
            );
        }
    }
}