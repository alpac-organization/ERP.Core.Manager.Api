using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Handlers
{
    #pragma warning disable CA1873 
    public class DeleteCostCenterHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<RegisterCostCenterHandler> _logger) : IRequestHandler<DeleteCostCenterCommand, bool>
    {
        public async Task<bool> Handle(DeleteCostCenterCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚩Iniciando proceso de eliminación del centro de costo con id: {identification}", request.CostCenterId);

            var area = await _unitOfWork.WorkAreas.Entities
                .Where(area => area.IsActive)
                .Where(area => area.CompanyId == request.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (area is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Esta area de trabajo no existe, porfavor seleccionar un area correcta", "ERP:AreaNotFound");
            }

            var costCenter = await _unitOfWork.CostCenters.Entities
                .Where(cost => cost.IsActive)
                .Where(cost => cost.WorkAreaId == area.Id)
                .Where(cost => cost.Id == request.CostCenterId)
                .FirstOrDefaultAsync(cancellationToken);

            if (costCenter is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Este centro de costo no existe","ERP:CostoCenterNotFound");
            }

            costCenter.IsActive = false;
            costCenter.DeletedAt = DateTime.Now;

            await _unitOfWork.CostCenters.UpdateAsync(costCenter);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅Centro de costo eliminado con exito.");

            return true;
        }
    }

    #pragma warning restore CA1873
}