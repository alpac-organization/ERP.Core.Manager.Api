using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Handlers
{
    public class RegisterCostCenterHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<RegisterCostCenterHandler> _logger) : IRequestHandler<RegisterCostCenterCommand>
    {
        public async Task Handle(RegisterCostCenterCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚩Iniciando proceso de registro de centro de costo");

            var area = await _unitOfWork.WorkAreas.Entities
                .Where(area => area.Id == request.AreaId)
                .Where(area => area.CompanyId == request.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (area is null)
            {
                _errorManager.ThrowBadRequest("Esta area no se encuentra registrada", "ERP:AreaNotFound");
            }
            
            await _unitOfWork.CostCenters.RegisterCostCenter(new()
            {
                WorkAreaId = request.AreaId,
                CostCenterName = request.CostCenterName,
                Description = request?.Description ?? "Sin Descripción",
                IsActive = true
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("✅Centro de costo registrado con exito");
        }
    }
}