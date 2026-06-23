using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Handlers
{
    public class RegisterWorkAreaHandler(IUnitOfWork _unitOfWork, ILogger<RegisterWorkAreaHandler> _logger) : IRequestHandler<RegisterWorkAreaCommand>
    {
        public async Task Handle(RegisterWorkAreaCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚩Iniciando proceso de registro de area de trabajo: \n\nData inicial: {@request}", request);

            var existingWorkAreas = await _unitOfWork.WorkAreas.Entities
                .Where(x => x.CompanyId == request.CompanyId)
                .ToListAsync(cancellationToken);

            int maxCode = existingWorkAreas.Any()
                ? existingWorkAreas.Max(x => x.WorkAreaCode)
                : 0;

            await _unitOfWork.WorkAreas.RegisterWorkArea(new()
            {
                CompanyId = request.CompanyId,
                WorkAreaCode = maxCode + 1,
                IsActive = true,
                WorkAreaName = request.WorkAreaName,
                Description = request?.Description ?? "Sin Descripción",
                WorkAreaCode = 0
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("✅Area de trabajo registrada con exito");
        }
    }
}