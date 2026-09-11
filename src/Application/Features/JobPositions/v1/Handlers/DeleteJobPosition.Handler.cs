using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Handlers
{
    public class DeleteJobPositionHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<DeleteJobPositionHandler> _logger) : BaseValidatorHandler<DeleteJobPositionCommand, Unit>(_unitOfWork, _errorManager)
    {
        public override async Task<Unit> Handle(DeleteJobPositionCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            if (access.Role?.RoleType != RoleType.Administrator)
            {
                return _errorManager.ThrowBadRequest<Unit>("Solo administradores puede eliminar un dato del catalogo", "ERP:DeleteJobPosition");
            }  

            _logger.LogInformation("🚩Iniciando proceso de eliminación de cargo");

            var jobPosition = await _unitOfWork.JobPositions.Entities
                .Where(job => job.IsActive)
                .Where(job => job.Id == request.JobPositionId)
                .Where(job => job.CompanyId == request.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (jobPosition is null)
            {
                return _errorManager.ThrowBadRequest<Unit>("Este cargo no se encuentra registrado!", "ERP");
            }

            jobPosition.DeletedAt = DateTime.Now;
            jobPosition.IsActive = false;

            await _unitOfWork.JobPositions.UpdateAsync(jobPosition);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("✅Cargo eliminado con exito");

            return Unit.Value;
        }
    }
}