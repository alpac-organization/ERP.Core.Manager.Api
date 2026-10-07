using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers;

public class UpdateSupplierExclusiveStatusHandler(
    IUnitOfWork _unitOfWork,
    ILogger<UpdateSupplierExclusiveStatusHandler> _logger,
    IErrorManager _errorManager)
    : BaseValidatorHandler<UpdateSupplierExclusiveStatusCommand, bool>(_unitOfWork, _errorManager)
{
    public override async Task<bool> Handle(UpdateSupplierExclusiveStatusCommand request, CancellationToken cancellationToken)
    {
        var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

        if (!access.IsSuccess)
        {
            return access.ErrorResponse;
        }

        if (access.Role?.RoleType is RoleType.Operator or RoleType.Manager)
        {
            _logger.LogWarning(
                "Usuario {UserId} con rol {RoleType} intentó actualizar ExclusiveStatus del proveedor {SupplierId}",
                request.UserId,
                access.Role.RoleType,
                request.SupplierId);

            return _errorManager.ThrowBadRequest<bool>(
                "No tienes permiso para aprobar o rechazar la exclusividad de un proveedor.",
                "ERP:01");
        }

        var supplierDetails = await _unitOfWork.SuppliersDetails.Entities
            .Include(d => d.Supplier)
            .FirstOrDefaultAsync(d => d.SupplierId == request.SupplierId && d.DeletedAt == null, cancellationToken);

        if (supplierDetails is null || supplierDetails.Supplier is null || !supplierDetails.Supplier.IsActive)
        {
            return _errorManager.ThrowBadRequest<bool>("El proveedor no existe o no está activo.", "ERP:04");
        }

        if (supplierDetails.ExclusiveStatus != SupplierExclusiveStatus.PendingReview)
        {
            return _errorManager.ThrowBadRequest<bool>(
                "Solo se pueden aprobar o rechazar proveedores en estado PendingReview.",
                "ERP:EXCLUSIVE_01");
        }

        supplierDetails.ExclusiveStatus = request.ExclusiveStatus;

        await _unitOfWork.SuppliersDetails.UpdateAsync(supplierDetails);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Usuario {UserId} actualizó ExclusiveStatus del proveedor {SupplierId} a {Status}. Comments: {Comments}",
            request.UserId,
            request.SupplierId,
            request.ExclusiveStatus,
            request.Comments);

        return true;
    }
}
