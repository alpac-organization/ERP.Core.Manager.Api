using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Handlers
{
    public class DeleteJobPositionHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<DeleteJobPositionHandler> _logger) : IRequestHandler<DeleteJobPositionCommand, bool>
    {
        public async Task<bool> Handle(DeleteJobPositionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚩Iniciando proceso de eliminación de cargo");

            var jobPosition = await _unitOfWork.JobPositions.Entities
                .Where(job => job.IsActive)
                .Where(job => job.Id == request.JobPositionId)
                .Where(job => job.CompanyId == request.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (jobPosition is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Este cargo no se encuentra registrado!", "ERP");
            }

            jobPosition.DeletedAt = DateTime.Now;
            jobPosition.IsActive = false;

            await _unitOfWork.JobPositions.UpdateAsync(jobPosition);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("✅Cargo eliminado con exito");
            return true;
        }
    }
}