using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Handlers
{
    public class GetCostCenterByAreaHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper) : IRequestHandler<GetCostCentersByAreaQuery, List<CostCenterDto>>
    {
        public async Task<List<CostCenterDto>> Handle(GetCostCentersByAreaQuery request, CancellationToken cancellationToken)
        {
            var area =  await _unitOfWork.WorkAreas.Entities
                .Where(area => area.Id == request.AreaId)
                .Where(area => area.CompanyId == request.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (area is null)
            {
                return _errorManager.ThrowBadRequest<List<CostCenterDto>>("El area seleccionada no existe", "ERP:AreaNotFound");
            }

            var costCenters = _unitOfWork.CostCenters.Entities    
                .Where(cost => cost.IsActive)
                .Where(cost => cost.WorkAreaId == area.Id)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<CostCenterDto>>(costCenters);
        }
    }
}