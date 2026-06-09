using MediatR;
using Microsoft.Extensions.Logging;

using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Dtos;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

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
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("✅Area de trabajo obtenidas con exito");
            return _mapper.Map<List<WorkAreaDto>>(workAreas);            
        }
    }
}