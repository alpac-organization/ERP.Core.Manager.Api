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
            .AsSplitQuery()
            .Include(p => p.SupplierProducts)
                .ThenInclude(sp => sp.PriceHistories)
            .Include(p => p.SupplierProducts)
                .ThenInclude(sp => sp.TierPrices)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.DeletedAt == null, cancellationToken);

        if (product is null)
        {
            return _errorManager.ThrowBadRequest<bool>("El producto no existe.", "ERP:PROD_NOT_FOUND");
        }

        var categoryChanged = request.CategoryId.HasValue && request.CategoryId.Value != product.CategoryId;
        if (categoryChanged)
        {
            var categoryExists = await _unitOfWork.CategoryProducts.Entities
                .AnyAsync(c => c.Id == request.CategoryId!.Value && c.IsActive && c.DeletedAt == null, cancellationToken);

            if (!categoryExists)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    "La categoría seleccionada no existe o no está activa.",
                    "ERP:002");
            }
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
            var linkError = await SupplierProductLinkValidator.ValidateSuppliersToLinkAsync(
                _unitOfWork,
                request.Suppliers,
                alreadyLinkedSupplierIds: null,
                cancellationToken);

            if (linkError is not null)
            {
                return _errorManager.ThrowBadRequest<bool>(linkError.Message, linkError.Code);
            }

            var now = DateTime.UtcNow;
            var supplierIds = request.Suppliers.Select(s => s.SupplierId).Distinct().ToList();
            var supplierCurrencies = await _unitOfWork.SuppliersDetails.Entities
                .AsNoTracking()
                .Where(d => supplierIds.Contains(d.SupplierId))
                .ToDictionaryAsync(d => d.SupplierId, d => d.Currency, cancellationToken);

            foreach (var supplierItem in request.Suppliers)
            {
                var supplierLinks = product.SupplierProducts
                    .Where(sp => sp.SupplierId == supplierItem.SupplierId)
                    .ToList();

                var existingLink = supplierLinks.FirstOrDefault(sp => sp.IsActive && sp.DeletedAt == null)
                    ?? supplierLinks.FirstOrDefault();

                var fallbackCurrency = supplierCurrencies.GetValueOrDefault(supplierItem.SupplierId, Currency.NIO);

                if (existingLink is not null)
                {
                    existingLink.IsActive = true;
                    existingLink.DeletedAt = null;

                    var applyError = SupplierProductPriceHelper.ApplyExistingLinkUpdate(
                        existingLink,
                        supplierItem.UnitPrice,
                        supplierItem.TierPrices,
                        supplierItem.UnitMeasureId,
                        product.UnitMeasureId,
                        now,
                        supplierItem.Currency);

                    if (applyError is not null)
                    {
                        return _errorManager.ThrowBadRequest<bool>(applyError, "ERP:PROD07");
                    }

                    continue;
                }

                var missingPreferential = SupplierProductPriceHelper.MissingNewPreferentialMessage(
                    supplierItem.TierPrices?.FirstOrDefault());
                if (missingPreferential is not null)
                {
                    return _errorManager.ThrowBadRequest<bool>(missingPreferential, "ERP:PROD07");
                }

                product.SupplierProducts.Add(
                    SupplierProductPriceHelper.BuildSupplierProduct(
                        supplierItem.SupplierId,
                        productId: null,
                        supplierItem.UnitPrice,
                        supplierItem.TierPrices,
                        supplierItem.UnitMeasureId ?? product.UnitMeasureId,
                        now,
                        supplierItem.Currency ?? fallbackCurrency));
            }
        }

        if (categoryChanged)
        {
            var newCategoryId = request.CategoryId!.Value;
            var oldCategoryId = product.CategoryId;
            var oldCode = product.Code;

            await _unitOfWork.ExecuteWithUniqueCodeRetryAsync(async token =>
            {
                var (isSuccess, code) = await _codeGenerator.GenerateUniqueProductCode(
                    access.Profile,
                    newCategoryId,
                    token
                );

                if (!isSuccess || string.IsNullOrWhiteSpace(code))
                {
                    _errorManager.ThrowBadRequest<bool>(
                        "No se pudo regenerar el código del producto para la nueva categoría.",
                        "ERP:PROD01");
                    return;
                }

                _logger.LogInformation(
                    "Producto {ProductId} cambia de categoría {OldCategory} -> {NewCategory}. Código {OldCode} -> {NewCode}",
                    product.Id,
                    oldCategoryId,
                    newCategoryId,
                    oldCode,
                    code);

                product.CategoryId = newCategoryId;
                product.Code = code;

                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync(token);
            }, cancellationToken);
        }
        else
        {
            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Usuario {UserId} actualizó el producto {ProductId}", request.UserId, product.Id);

        return true;
    }
}
