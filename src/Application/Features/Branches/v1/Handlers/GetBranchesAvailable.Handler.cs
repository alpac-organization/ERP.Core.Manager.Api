using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Application.Features.Branches.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Branches.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Branches.v1.Handlers
{
    public class GetBranchesAvailableHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<GetBranchesAvailableQuery, List<BranchesDto>>
    {
        public async Task<List<BranchesDto>> Handle(GetBranchesAvailableQuery request, CancellationToken cancellationToken)
        {
            var branchesQuery = _unitOfWork.Branches.Entities
                .Where(branch => branch.IsActive)
                .Where(branch => branch.CompanyId == request.CompanyId)
                .AsNoTracking();

            if (request.HasWarehouse.HasValue)
            {
                branchesQuery = branchesQuery
                    .Where(branch => branch.HasWarehouse);
            }

            var branches = await branchesQuery
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<BranchesDto>>(branches);
        }
    }
}