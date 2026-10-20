using MediatR;
using Microsoft.Extensions.Logging;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Commons.Mappings;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers;

public class RegisterReasonsInventoryRemovalHandler(
    IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<RegisterReasonsInventoryRemovalHandler> _logger)
        : BaseValidatorHandler<RegisterReasonsInventoryRemovalCommand, Unit>(_unitOfWork, _errorManager)
{
    public override async Task<Unit> Handle(RegisterReasonsInventoryRemovalCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🚩Iniciando proceso de registro de concepto de salida de inventario: \n\nData inicial: {@request}", request);

        var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);
        if (!access.IsSuccess) return access.ErrorResponse!;

        await _unitOfWork.ReasonsInventoryRemoval.RegisterNewReasonInventoryRemoval(
            request.ToReasonsInventoryRemovalEntity());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("✅Concepto de salida de inventario registrado con exito");

        return Unit.Value;
    }
}
