using MediatR;
using Microsoft.Extensions.Logging;

using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Handlers
{
    public class RegisterWorkAreaHandler(IUnitOfWork _unitOfWork, ILogger<RegisterWorkAreaHandler> _logger) : IRequestHandler<RegisterWorkAreaCommand>
    {
        public async Task Handle(RegisterWorkAreaCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚩Iniciando proceso de registro de area de trabajo");

            await _unitOfWork.WorkAreas.RegisterWorkArea(new()
            {
                CompanyId = request.CompanyId,
                IsActive = true,
                WorkAreaName = request.WorkAreaName,
                Description = request?.Description ?? "Sin Descripción"
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("✅Area de trabajo registrada con exito");
        }
    }
}