using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Application.Commons.Interfaces.Services;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers;

public class UpdateProductHandler(
    IUnitOfWork _unitOfWork,
    ILogger<UpdateProductHandler> _logger,
    IErrorManager _errorManager,
    ICodeGenerator _codeGenerator)
    : BaseValidatorHandler<UpdateProductCommand, bool>(_unitOfWork, _errorManager)
{
    public override async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

        if (!access.IsSuccess)
        {
            return access.ErrorResponse;
        }

        if (access.Role?.RoleType == RoleType.Supervisor)
        {
            return _errorManager.ThrowBadRequest<bool>("No tienes permiso para actualizar productos.", "ERP:01");
        }

        var product = await _unitOfWork.Products.Entities
            .Include(p => p.SupplierProducts)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.DeletedAt == null, cancellationToken);

        if (product is null)
        {
            return _errorManager.ThrowBadRequest<bool>("El producto no existe.", "ERP:PROD_NOT_FOUND");
        }

        if (request.CategoryId.HasValue && request.CategoryId.Value != product.CategoryId)
        {
            var categoryExists = await _unitOfWork.CategoryProducts.Entities
                .AnyAsync(c => c.Id == request.CategoryId.Value && c.IsActive && c.DeletedAt == null, cancellationToken);

            if (!categoryExists)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    "La categoría seleccionada no existe o no está activa.",
                    "ERP:002");
            }

            var (isSuccess, code) = await _codeGenerator.GenerateUniqueProductCode(
                request.CompanyId,
                request.CategoryId.Value,
                cancellationToken);

            if (!isSuccess || string.IsNullOrWhiteSpace(code))
            {
                return _errorManager.ThrowBadRequest<bool>(
                    "No se pudo regenerar el código del producto para la nueva categoría.",
                    "ERP:PROD01");
            }

            _logger.LogInformation(
                "Producto {ProductId} cambia de categoría {OldCategory} -> {NewCategory}. Código {OldCode} -> {NewCode}",
                product.Id,
                product.CategoryId,
                request.CategoryId.Value,
                product.Code,
                code);

            product.CategoryId = request.CategoryId.Value;
            product.Code = code;
        }

        if (request.UnitMeasureId.HasValue && request.UnitMeasureId.Value != product.UnitMeasureId)
        {
            var unitMeasureExists = await _unitOfWork.UnitsMeasurement.Entities
                .AnyAsync(u => u.Id == request.UnitMeasureId.Value && u.IsActive, cancellationToken);

            if (!unitMeasureExists)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    "La unidad de medida seleccionada no existe.",
                    "ERP:PROD02");
            }

            product.UnitMeasureId = request.UnitMeasureId.Value;
        }

        product.ProductName = request.ProductName ?? product.ProductName;
        product.Description = request.Description ?? product.Description;
        product.ProductUsageType = request.ProductUsageType ?? product.ProductUsageType;
        product.IsTaxExempt = request.IsTaxExempt ?? product.IsTaxExempt;

        if (request.Suppliers is { Count: > 0 })
        {
            var supplierIds = request.Suppliers.Select(s => s.SupplierId).Distinct().ToList();

            if (supplierIds.Count != request.Suppliers.Count)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    "No se puede relacionar el mismo proveedor más de una vez al producto.",
                    "ERP:PROD03");
            }

            var alreadyLinked = product.SupplierProducts
                .Where(sp => sp.IsActive && sp.DeletedAt == null)
                .Select(sp => sp.SupplierId)
                .ToHashSet();

            if (supplierIds.Any(alreadyLinked.Contains))
            {
                return _errorManager.ThrowBadRequest<bool>(
                    "Uno o más proveedores ya están vinculados a este producto.",
                    "ERP:PROD07");
            }

            var existingSupplierCount = await _unitOfWork.Suppliers.Entities
                .CountAsync(s => supplierIds.Contains(s.Id) && s.IsActive && s.DeletedAt == null, cancellationToken);

            if (existingSupplierCount != supplierIds.Count)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    "Uno o más proveedores seleccionados no existen o no están activos.",
                    "ERP:PROD04");
            }

            var unitMeasureIds = request.Suppliers
                .SelectMany(s => s.TierPrices ?? [])
                .Where(t => t.UnitMeasureId.HasValue)
                .Select(t => t.UnitMeasureId!.Value)
                .Distinct()
                .ToList();

            if (unitMeasureIds.Count > 0)
            {
                var existingUomCount = await _unitOfWork.UnitsMeasurement.Entities
                    .CountAsync(u => unitMeasureIds.Contains(u.Id) && u.IsActive, cancellationToken);

                if (existingUomCount != unitMeasureIds.Count)
                {
                    return _errorManager.ThrowBadRequest<bool>(
                        "Una o más unidades de medida no existen o no están activas.",
                        "ERP:PROD06");
                }
            }

            foreach (var supplierItem in request.Suppliers)
            {
                var overlapError = SupplierProductPriceHelper.ValidateTierPriceOverlaps(
                    supplierItem.TierPrices ?? []);

                if (overlapError is not null)
                {
                    return _errorManager.ThrowBadRequest<bool>(overlapError, "ERP:PROD05");
                }
            }

            var now = DateTime.UtcNow;

            foreach (var supplierItem in request.Suppliers)
            {
                product.SupplierProducts.Add(
                    SupplierProductPriceHelper.BuildSupplierProduct(
                        supplierItem.SupplierId,
                        productId: null,
                        supplierItem.UnitPrice,
                        supplierItem.TierPrices,
                        product.UnitMeasureId,
                        now));
            }
        }

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Usuario {UserId} actualizó el producto {ProductId}", request.UserId, product.Id);

        return true;
    }
}
