using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Handlers
{
    public class GetJobPositionsHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<GetJobPositionsQuery, List<JobPositionDto>>
    {
        public async Task<List<JobPositionDto>> Handle(GetJobPositionsQuery request, CancellationToken cancellationToken)
        {
            var jobPositions = await _unitOfWork.JobPositions.Entities    
                .Where(cost => cost.IsActive)
                .Where(cost => cost.CompanyId == request.CompanyId)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<JobPositionDto>>(jobPositions);
        }
    }
}
