using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Commands;

using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Services;
using System.ComponentModel;

namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Handlers
{
    public class RegisterWorkAreaHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<RegisterWorkAreaHandler> _logger, ICodeGenerator _codeGenerator) : BaseValidatorHandler<RegisterWorkAreaCommand, Unit>(_unitOfWork, _errorManager)
    {
        public override async Task<Unit> Handle(RegisterWorkAreaCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚩Iniciando proceso de registro de area de trabajo: \n\nData inicial: {@request}", request);

            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            if (access.Role?.RoleType != RoleType.Administrator)
            {
                return _errorManager.ThrowBadRequest<Unit>("Solo administradores pueden registrar áreas de trabajo", "ERP:RegisterWorkArea");
            }

            var existingWorkAreas = await _unitOfWork.WorkAreas.Entities
                .Where(x => x.CompanyId == request.CompanyId)
                .ToListAsync(cancellationToken);

            var (IsSuccess, newCode) = await _codeGenerator.GenerateUniqueWorkAreaCodeAsync(
                    request.CompanyId, cancellationToken);

            if (!IsSuccess)
            {
                return _errorManager.ThrowBadRequest<Unit>("No se pudo generar el código del Área de trabajo.",
                "ERP:WORK_AREA_CODE_GENERATION_FAILED");
            }

            await _unitOfWork.WorkAreas.RegisterWorkArea(new()
            {
                CompanyId = request.CompanyId,
                WorkAreaCode = newCode,
                IsActive = true,
                WorkAreaName = request.WorkAreaName,
                Description = request?.Description ?? "Sin Descripción"
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅Area de trabajo registrada con exito");

            return Unit.Value;
        }
    }
}