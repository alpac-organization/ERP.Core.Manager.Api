using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Branches.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Branches.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Branches.v1.Handlers
{
    public class GetBranchesAvailableHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<GetBranchesAvailableQuery, List<BranchesDto>>
    {
        public async Task<List<BranchesDto>> Handle(GetBranchesAvailableQuery request, CancellationToken cancellationToken)
        {
            var branches = await _unitOfWork.Branches.Entities
                .Where(branch => branch.CompanyId == request.CompanyId)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<BranchesDto>>(branches);
        }
    }
}