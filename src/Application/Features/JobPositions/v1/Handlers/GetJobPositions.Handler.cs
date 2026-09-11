using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Handlers
{
    public class GetJobPositionsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper) : BaseValidatorHandler<GetJobPositionsQuery, List<JobPositionDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<List<JobPositionDto>> Handle(GetJobPositionsQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }            

            var jobPositions = await _unitOfWork.JobPositions.Entities    
                .Where(job => job.IsActive)
                .Where(job => job.CompanyId == request.CompanyId)
                .OrderByDescending(job => job.CreatedAt)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<JobPositionDto>>(jobPositions);
        }
    }
}
