using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Commands;

using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Handlers
{
    public class DeleteWorkAreaHandler(IUnitOfWork _unitOfWork, ILogger<RegisterWorkAreaHandler> _logger, IErrorManager _errorManager) : BaseValidatorHandler<DeleteWorkAreaCommand, Unit>(_unitOfWork, _errorManager)
    {
        public override async Task<Unit> Handle(DeleteWorkAreaCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚩Iniciando proceso para eleminar area con id: {@id}", request.WorkAreaId);

            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            if (access.Role?.RoleType != RoleType.Administrator)
            {
                return _errorManager.ThrowBadRequest<Unit>("Solo administradores pueden eliminar áreas de trabajo", "ERP:DeleteWorkArea");
            }

            var area = await _unitOfWork.WorkAreas.Entities
                .Where(col => col.Id == request.WorkAreaId)
                .Where(col => col.CompanyId== request.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (area is null)
            {
                return _errorManager.ThrowBadRequest<Unit>("Esta area no existe", "ERP");
            }

            area.DeletedAt = DateTime.Now;
            area.IsActive = false;

            await _unitOfWork.WorkAreas.UpdateAsync(area);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅Area de trabajo eliminada con exito");

            return Unit.Value;
        }
    }
}