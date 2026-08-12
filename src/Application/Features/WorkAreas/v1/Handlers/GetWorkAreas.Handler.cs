using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Dtos;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Handlers
{
    public class GetWorkAreaHandler(IUnitOfWork _unitOfWork, IMapper _mapper, ILogger<RegisterWorkAreaHandler> _logger) : IRequestHandler<GetWorkAreasQuery, List<WorkAreaDto>>
    {
        public async Task<List<WorkAreaDto>> Handle(GetWorkAreasQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚩Obteniendos areas de trabajos");

            var workAreas = await _unitOfWork.WorkAreas.Entities
                .Where(wk => wk.CompanyId == request.CompanyId)
                .Where(wk => wk.IsActive)
                .Include(wk => wk.CostCenters
                    .Where(cc => 
                        cc.IsActive
                    )
                )
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("✅Area de trabajo obtenidas con exito");
            
            var areasMapped = _mapper.Map<List<WorkAreaDto>>(workAreas);     

            return areasMapped;       
        }
    }
}