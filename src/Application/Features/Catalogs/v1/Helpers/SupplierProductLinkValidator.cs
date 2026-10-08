using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;

public sealed record LinkValidationError(string Message, string Code);

public static class SupplierProductLinkValidator
{
    public static async Task<LinkValidationError?> ValidateSuppliersToLinkAsync(
        IUnitOfWork unitOfWork,
        IReadOnlyList<ProductSupplierItemDto> suppliers,
        ISet<Guid>? alreadyLinkedSupplierIds,
        CancellationToken cancellationToken)
    {
        if (suppliers.Count == 0)
        {
            return null;
        }

        var supplierIds = suppliers.Select(s => s.SupplierId).Distinct().ToList();

        if (supplierIds.Count != suppliers.Count)
        {
            return new LinkValidationError(
                "No se puede relacionar el mismo proveedor más de una vez al producto.",
                "ERP:PROD03");
        }

        if (alreadyLinkedSupplierIds is not null && supplierIds.Any(alreadyLinkedSupplierIds.Contains))
        {
            return new LinkValidationError(
                "Uno o más proveedores ya están vinculados a este producto.",
                "ERP:PROD07");
        }

        var existingSupplierCount = await unitOfWork.Suppliers.Entities
            .CountAsync(s => supplierIds.Contains(s.Id) && s.IsActive && s.DeletedAt == null, cancellationToken);

        if (existingSupplierCount != supplierIds.Count)
        {
            return new LinkValidationError(
                "Uno o más proveedores seleccionados no existen o no están activos.",
                "ERP:PROD04");
        }

        var unitMeasureIds = suppliers
            .SelectMany(s =>
            {
                var ids = (s.TierPrices ?? [])
                    .Where(t => t.UnitMeasureId.HasValue)
                    .Select(t => t.UnitMeasureId!.Value);

                return s.UnitMeasureId.HasValue
                    ? ids.Append(s.UnitMeasureId.Value)
                    : ids;
            })
            .Distinct()
            .ToList();

        var unitMeasureError = await ValidateUnitMeasuresAsync(
            unitOfWork,
            unitMeasureIds,
            "ERP:PROD06",
            cancellationToken);

        if (unitMeasureError is not null)
        {
            return unitMeasureError;
        }

        foreach (var supplierItem in suppliers)
        {
            var overlapError = SupplierProductPriceHelper.ValidateTierPriceOverlaps(
                supplierItem.TierPrices ?? []);

            if (overlapError is not null)
            {
                return new LinkValidationError(overlapError, "ERP:PROD05");
            }
        }

        return null;
    }

    public static async Task<(LinkValidationError? Error, Dictionary<Guid, Guid> ProductUnitMeasures)> ValidateProductsToLinkAsync(
        IUnitOfWork unitOfWork,
        IReadOnlyList<SupplierProductItemDto> products,
        ISet<Guid>? alreadyLinkedProductIds,
        string errorCode,
        CancellationToken cancellationToken)
    {
        Dictionary<Guid, Guid> productUnitMeasures = [];

        if (products.Count == 0)
        {
            return (null, productUnitMeasures);
        }

        var productIds = products.Select(p => p.ProductId).Distinct().ToList();

        if (productIds.Count != products.Count)
        {
            return (
                new LinkValidationError(
                    "No se puede relacionar el mismo producto más de una vez al proveedor.",
                    errorCode),
                productUnitMeasures);
        }

        if (alreadyLinkedProductIds is not null && productIds.Any(alreadyLinkedProductIds.Contains))
        {
            return (
                new LinkValidationError(
                    "Uno o más productos ya están vinculados a este proveedor.",
                    errorCode),
                productUnitMeasures);
        }

        var productRows = await unitOfWork.Products.Entities
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && p.DeletedAt == null)
            .Select(p => new { p.Id, p.UnitMeasureId })
            .ToListAsync(cancellationToken);

        if (productRows.Count != productIds.Count)
        {
            return (
                new LinkValidationError(
                    "Uno o más productos seleccionados no existen.",
                    errorCode),
                productUnitMeasures);
        }

        productUnitMeasures = productRows.ToDictionary(p => p.Id, p => p.UnitMeasureId);

        var unitMeasureIds = products
            .SelectMany(p =>
            {
                var ids = (p.TierPrices ?? [])
                    .Where(t => t.UnitMeasureId.HasValue)
                    .Select(t => t.UnitMeasureId!.Value);

                return p.UnitMeasureId.HasValue
                    ? ids.Append(p.UnitMeasureId.Value)
                    : ids;
            })
            .Distinct()
            .ToList();

        var unitMeasureError = await ValidateUnitMeasuresAsync(
            unitOfWork,
            unitMeasureIds,
            errorCode,
            cancellationToken);

        if (unitMeasureError is not null)
        {
            return (unitMeasureError, productUnitMeasures);
        }

        foreach (var productItem in products)
        {
            var overlapError = SupplierProductPriceHelper.ValidateTierPriceOverlaps(
                productItem.TierPrices ?? []);

            if (overlapError is not null)
            {
                return (new LinkValidationError(overlapError, errorCode), productUnitMeasures);
            }
        }

        return (null, productUnitMeasures);
    }

    private static async Task<LinkValidationError?> ValidateUnitMeasuresAsync(
        IUnitOfWork unitOfWork,
        IReadOnlyList<Guid> unitMeasureIds,
        string errorCode,
        CancellationToken cancellationToken)
    {
        if (unitMeasureIds.Count == 0)
        {
            return null;
        }

        var existingUomCount = await unitOfWork.UnitsMeasurement.Entities
            .CountAsync(u => unitMeasureIds.Contains(u.Id) && u.IsActive, cancellationToken);

        if (existingUomCount != unitMeasureIds.Count)
        {
            return new LinkValidationError(
                "Una o más unidades de medida no existen o no están activas.",
                errorCode);
        }

        return null;
    }
}
