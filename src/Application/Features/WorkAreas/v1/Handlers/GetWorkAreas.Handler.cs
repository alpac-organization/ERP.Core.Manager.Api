using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Queries
;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Handlers
{
    public class GetWorkAreaHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper, ILogger<RegisterWorkAreaHandler> _logger) : BaseValidatorHandler<GetWorkAreasQuery, List<WorkAreaDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<List<WorkAreaDto>> Handle(GetWorkAreasQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚩Obteniendos areas de trabajos");

            var workAreas = await _unitOfWork.WorkAreas.Entities
                .Where(wk => wk.IsActive)
                .Where(wk => wk.CompanyId == request.CompanyId)
                .OrderByDescending(wk => wk.CreatedAt)
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("✅Area de trabajo obtenidas con exito");
            
            var areasMapped = _mapper.Map<List<WorkAreaDto>>(workAreas);     

            return areasMapped;       
        }
    }
}