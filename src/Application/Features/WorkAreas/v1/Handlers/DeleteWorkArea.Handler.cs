using MediatR;
using Microsoft.Extensions.Logging;

using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Commands;
using ERP.Core.Application.Commons.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Handlers
{
    public class DeleteWorkAreaHandler(IUnitOfWork _unitOfWork, ILogger<RegisterWorkAreaHandler> _logger, IErrorManager _errorManager) : IRequestHandler<DeleteWorkAreaCommand, bool>
    {
        public async Task<bool> Handle(DeleteWorkAreaCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚩Iniciando proceso para eleminar area con id: {@id}", request.WorkAreaId);

            var area = await _unitOfWork.WorkAreas.Entities
                .Where(col => col.Id == request.WorkAreaId)
                .Where(col => col.CompanyId== request.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (area is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Esta area no existe", "ERP");
            }

            area.DeletedAt = DateTime.Now;
            area.IsActive = false;

            await _unitOfWork.WorkAreas.UpdateAsync(area);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅Area de trabajo eliminada con exito");

            return true;
        }
    }
}