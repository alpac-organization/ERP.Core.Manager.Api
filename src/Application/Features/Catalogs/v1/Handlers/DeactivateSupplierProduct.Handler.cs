using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers;

public class DeactivateSupplierProductHandler(
    IUnitOfWork _unitOfWork,
    ILogger<DeactivateSupplierProductHandler> _logger,
    IErrorManager _errorManager)
    : BaseValidatorHandler<DeactivateSupplierProductCommand, bool>(_unitOfWork, _errorManager)
{
    public override async Task<bool> Handle(DeactivateSupplierProductCommand request, CancellationToken cancellationToken)
    {
        var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

        if (!access.IsSuccess)
        {
            return access.ErrorResponse;
        }

        if (access.Role?.RoleType == RoleType.Supervisor)
        {
            return _errorManager.ThrowBadRequest<bool>(
                "No tienes permiso para desactivar la relación proveedor-producto.",
                "ERP:01");
        }

        var product = await _unitOfWork.Products.Entities
            .AsSplitQuery()
            .Include(p => p.SupplierProducts.Where(sp =>
                sp.SupplierId == request.SupplierId &&
                sp.DeletedAt == null))
            .ThenInclude(sp => sp.PriceHistories)
            .Include(p => p.SupplierProducts.Where(sp =>
                sp.SupplierId == request.SupplierId &&
                sp.DeletedAt == null))
            .ThenInclude(sp => sp.TierPrices)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.DeletedAt == null, cancellationToken);

        var supplierProduct = product?.SupplierProducts.FirstOrDefault(sp => sp.IsActive);

        if (product is null || supplierProduct is null)
        {
            return _errorManager.ThrowBadRequest<bool>(
                "La relación proveedor-producto no existe o ya está inactiva.",
                "ERP:LINK_01");
        }

        var now = DateTime.UtcNow;
        SupplierProductPriceHelper.SoftDeleteSupplierProduct(supplierProduct, now);

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Usuario {UserId} desactivó SupplierProduct {SupplierProductId}",
            request.UserId,
            supplierProduct.Id);

        return true;
    }
}
