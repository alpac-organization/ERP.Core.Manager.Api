using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Commands;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Handlers
{
    public class RegisterCostCenterHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<RegisterCostCenterHandler> _logger) : BaseValidatorHandler<RegisterCostCenterCommand, Unit>(_unitOfWork, _errorManager)
    {
        public override async Task<Unit> Handle(RegisterCostCenterCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚩Iniciando proceso de registro de centro de costo");

            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            if (access.Role?.RoleType != RoleType.Administrator)
            {
                return _errorManager.ThrowBadRequest<Unit>("Solo administradores pueden registrar centros de costo", "ERP:RegisterCostCenter");
            }

            var area = await _unitOfWork.WorkAreas.Entities
                .Where(area => area.IsActive)
                .Where(area => area.Id == request.AreaId)
                .Where(area => area.CompanyId == request.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (area is null)
            {
                _errorManager.ThrowBadRequest("Esta area no se encuentra registrada", "ERP:AreaNotFound");
            }

            var lastCostCenterCode = await _unitOfWork.CostCenters.Entities
                .OrderByDescending(cc => cc.CostCenterCode)
                .Select(cc => cc.CostCenterCode)
                .FirstOrDefaultAsync(cancellationToken);
                
            await _unitOfWork.CostCenters.RegisterCostCenter(new()
            {
                WorkAreaId = request.AreaId,
                CostCenterName = request.CostCenterName,
                CoilCode = request.CoilCode,
                Description = request?.Description ?? "Sin Descripción",
                IsActive = true,
                CostCenterCode = lastCostCenterCode += 1
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("✅Centro de costo registrado con exito");

            return Unit.Value;
        }
    }
}