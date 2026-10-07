using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Entities.Shopping;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers;

public class UpdateSupplierProductPriceHandler(
    IUnitOfWork _unitOfWork,
    ILogger<UpdateSupplierProductPriceHandler> _logger,
    IErrorManager _errorManager)
    : BaseValidatorHandler<UpdateSupplierProductPriceCommand, bool>(_unitOfWork, _errorManager)
{
    public override async Task<bool> Handle(UpdateSupplierProductPriceCommand request, CancellationToken cancellationToken)
    {
        var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

        if (!access.IsSuccess)
        {
            return access.ErrorResponse;
        }

        if (access.Role?.RoleType == RoleType.Supervisor)
        {
            return _errorManager.ThrowBadRequest<bool>(
                "No tienes permiso para actualizar precios proveedor-producto.",
                "ERP:01");
        }

        var product = await _unitOfWork.Products.Entities
            .AsSplitQuery()
            .Include(p => p.SupplierProducts.Where(sp =>
                sp.SupplierId == request.SupplierId &&
                sp.IsActive &&
                sp.DeletedAt == null))
            .ThenInclude(sp => sp.PriceHistories)
            .Include(p => p.SupplierProducts.Where(sp =>
                sp.SupplierId == request.SupplierId &&
                sp.IsActive &&
                sp.DeletedAt == null))
            .ThenInclude(sp => sp.TierPrices)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.DeletedAt == null, cancellationToken);

        var supplierProduct = product?.SupplierProducts.FirstOrDefault();

        if (product is null || supplierProduct is null)
        {
            return _errorManager.ThrowBadRequest<bool>(
                "La relación proveedor-producto no existe o no está activa.",
                "ERP:PRICE_01");
        }

        var now = DateTime.UtcNow;

        if (request.NewUnitPrice.HasValue)
        {
            SupplierProductPriceHelper.ApplyUnitPriceChange(supplierProduct, request.NewUnitPrice.Value, now);
        }

        if (request.TierPrices is { Count: > 0 })
        {
            var existingActiveTiers = supplierProduct.TierPrices
                .Where(t => t.DeletedAt == null)
                .Select(t => new TierPriceDto
                {
                    MinQuantity = t.MinQuantity,
                    PreferentialPrice = t.PreferentialPrice,
                    ValidFrom = t.ValidFrom,
                    ValidTo = t.ValidTo
                });

            var combined = existingActiveTiers.Concat(request.TierPrices).ToList();
            var overlapError = SupplierProductPriceHelper.ValidateTierPriceOverlaps(combined);

            if (overlapError is not null)
            {
                return _errorManager.ThrowBadRequest<bool>(overlapError, "ERP:PRICE_02");
            }

            foreach (var tier in request.TierPrices)
            {
                var activePreferential = supplierProduct.PriceHistories
                    .Where(h =>
                        h.DeletedAt == null &&
                        h.PriceType == SupplierPriceHistoryType.PreferentialPrice &&
                        h.MinQuantity == tier.MinQuantity &&
                        h.EffectiveTo == SupplierProductPriceHelper.ActiveEffectiveTo)
                    .ToList();

                foreach (var history in activePreferential)
                {
                    history.EffectiveTo = now;
                }

                supplierProduct.TierPrices.Add(
                    SupplierProductPriceHelper.CreateTierPrice(tier, product.UnitMeasureId));

                supplierProduct.PriceHistories.Add(new HistoryPrices
                {
                    PriceType = SupplierPriceHistoryType.PreferentialPrice,
                    Price = tier.PreferentialPrice,
                    MinQuantity = tier.MinQuantity,
                    EffectiveFrom = now,
                    EffectiveTo = SupplierProductPriceHelper.ActiveEffectiveTo
                });
            }

            supplierProduct.LastPriceUpdate = now;
        }

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Usuario {UserId} actualizó precios de SupplierProduct {SupplierProductId}",
            request.UserId,
            supplierProduct.Id);

        return true;
    }
}
