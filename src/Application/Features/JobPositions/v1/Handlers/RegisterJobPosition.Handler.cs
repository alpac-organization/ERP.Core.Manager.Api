using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Commands;
namespace ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Handlers
{
    public class RegisterJobPositionHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<RegisterJobPositionHandler> _logger) : BaseValidatorHandler<RegisterJobPositionCommand, Unit>(_unitOfWork, _errorManager)
    {
        public override async Task<Unit> Handle(RegisterJobPositionCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            if (access.Role!.RoleType != RoleType.Administrator)
            {
                return _errorManager.ThrowBadRequest<Unit>("No tienes permiso para registrar un cargo de trabajo", "ERP:01");
            }

            _logger.LogInformation("🚩Iniciando proceso de registro de cargo");

            var company = await _unitOfWork.Companies.Entities    
                .Where(com => com.IsActive)
                .Where(com => com.Id == request.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (company is null )
            {
                _errorManager.ThrowBadRequest("Esta empresa no existe en nuestro sistema!", "ERP:01");
            }

            await _unitOfWork.JobPositions.RegisterJobPosition(new()
            {
                IsActive = true,
                CompanyId = request.CompanyId,
                Description = request.Description ?? "Sin Descripción",
                JobPositionName = request.JobPositionName
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("✅Cargo registrado con exito");

            return Unit.Value;
        }
    }
}