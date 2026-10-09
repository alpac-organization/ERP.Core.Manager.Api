using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Shopping;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;

public static class SupplierProductPriceHelper
{
    public static readonly DateTime ActiveEffectiveTo = DateTime.MaxValue;

    public static SupplierProduct BuildSupplierProduct(
        Guid supplierId,
        Guid? productId,
        decimal unitPrice,
        IEnumerable<TierPriceDto>? tierPrices,
        Guid? unitMeasureId,
        DateTime now)
    {
        var link = new SupplierProduct
        {
            IsActive = true,
            SupplierId = supplierId,
            UnitPrice = unitPrice,
            LastPriceUpdate = now,
            PriceHistories =
            [
                new HistoryPrices
                {
                    PriceType = SupplierPriceHistoryType.UnitPrice,
                    Price = unitPrice,
                    EffectiveFrom = now,
                    EffectiveTo = ActiveEffectiveTo
                }
            ],
            TierPrices = []
        };

        if (productId.HasValue)
        {
            link.ProductId = productId.Value;
        }

        foreach (var tier in tierPrices ?? [])
        {
            var resolvedUnitMeasureId = tier.UnitMeasureId ?? unitMeasureId;
            link.TierPrices.Add(CreateTierPrice(tier, resolvedUnitMeasureId));
            link.PriceHistories.Add(new HistoryPrices
            {
                PriceType = SupplierPriceHistoryType.PreferentialPrice,
                Price = tier.PreferentialPrice,
                MinQuantity = tier.MinQuantity,
                EffectiveFrom = ResolvePreferentialEffectiveFrom(tier.ValidFrom, now),
                EffectiveTo = ResolvePreferentialEffectiveTo(tier.ValidTo)
            });
        }

        return link;
    }

    public static SupplierProductTierPrice CreateTierPrice(TierPriceDto tier, Guid? unitMeasureId) =>
        new()
        {
            MinQuantity = tier.MinQuantity,
            PreferentialPrice = tier.PreferentialPrice,
            ValidFrom = tier.ValidFrom,
            ValidTo = tier.ValidTo,
            UnitMeasureId = unitMeasureId
        };

    public static DateTime ResolvePreferentialEffectiveFrom(DateOnly validFrom, DateTime fallbackNow)
    {
        var from = validFrom.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        return from > fallbackNow ? from : fallbackNow;
    }

    public static DateTime ResolvePreferentialEffectiveTo(DateOnly? validTo) =>
        validTo.HasValue
            ? validTo.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc)
            : ActiveEffectiveTo;

    public static DateTime? ToApiEffectiveTo(DateTime effectiveTo) =>
        effectiveTo == ActiveEffectiveTo || effectiveTo == DateTime.MaxValue
            ? null
            : effectiveTo;

    public static bool IsCurrentPrice(DateTime effectiveFrom, DateTime? effectiveTo, DateTime utcNow) =>
        effectiveFrom <= utcNow && (effectiveTo is null || effectiveTo > utcNow);

    public static void CloseActivePriceHistories(SupplierProduct supplierProduct, DateTime closedAt)
    {
        foreach (var history in supplierProduct.PriceHistories
                     .Where(h => h.DeletedAt == null && h.EffectiveTo == ActiveEffectiveTo))
        {
            history.EffectiveTo = closedAt;
        }
    }

    public static void ApplyUnitPriceChange(SupplierProduct supplierProduct, decimal newUnitPrice, DateTime now)
    {
        if (supplierProduct.UnitPrice == newUnitPrice)
        {
            return;
        }

        var active = supplierProduct.PriceHistories
            .FirstOrDefault(h =>
                h.DeletedAt == null &&
                h.PriceType == SupplierPriceHistoryType.UnitPrice &&
                h.EffectiveTo == ActiveEffectiveTo);

        if (active is not null)
        {
            active.EffectiveTo = now;
        }

        supplierProduct.PriceHistories.Add(new HistoryPrices
        {
            PriceType = SupplierPriceHistoryType.UnitPrice,
            Price = newUnitPrice,
            EffectiveFrom = now,
            EffectiveTo = ActiveEffectiveTo
        });

        supplierProduct.UnitPrice = newUnitPrice;
        supplierProduct.LastPriceUpdate = now;
    }

    public static string? ApplyTierPrices(
        SupplierProduct supplierProduct,
        IReadOnlyList<TierPriceDto> tierPrices,
        Guid fallbackUnitMeasureId,
        DateTime now)
    {
        if (tierPrices.Count == 0)
        {
            return null;
        }

        var existingActiveTiers = supplierProduct.TierPrices
            .Where(t => t.DeletedAt == null)
            .Select(t => new TierPriceDto
            {
                MinQuantity = t.MinQuantity,
                PreferentialPrice = t.PreferentialPrice,
                ValidFrom = t.ValidFrom,
                ValidTo = t.ValidTo,
                UnitMeasureId = t.UnitMeasureId
            });

        var overlapError = ValidateTierPriceOverlaps(existingActiveTiers.Concat(tierPrices));
        if (overlapError is not null)
        {
            return overlapError;
        }

        foreach (var tier in tierPrices)
        {
            var activePreferential = supplierProduct.PriceHistories
                .Where(h =>
                    h.DeletedAt == null &&
                    h.PriceType == SupplierPriceHistoryType.PreferentialPrice &&
                    h.MinQuantity == tier.MinQuantity &&
                    h.EffectiveTo == ActiveEffectiveTo)
                .ToList();

            foreach (var history in activePreferential)
            {
                history.EffectiveTo = now;
            }

            var resolvedUnitMeasureId = tier.UnitMeasureId ?? fallbackUnitMeasureId;

            supplierProduct.TierPrices.Add(CreateTierPrice(tier, resolvedUnitMeasureId));
            supplierProduct.PriceHistories.Add(new HistoryPrices
            {
                PriceType = SupplierPriceHistoryType.PreferentialPrice,
                Price = tier.PreferentialPrice,
                MinQuantity = tier.MinQuantity,
                EffectiveFrom = ResolvePreferentialEffectiveFrom(tier.ValidFrom, now),
                EffectiveTo = ResolvePreferentialEffectiveTo(tier.ValidTo)
            });
        }

        supplierProduct.LastPriceUpdate = now;
        return null;
    }

    public static string? ApplyExistingLinkUpdate(
        SupplierProduct supplierProduct,
        decimal unitPrice,
        IReadOnlyList<TierPriceDto>? tierPrices,
        Guid fallbackUnitMeasureId,
        DateTime now)
    {
        ApplyUnitPriceChange(supplierProduct, unitPrice, now);

        if (tierPrices is not { Count: > 0 })
        {
            return null;
        }

        return ApplyTierPrices(supplierProduct, tierPrices, fallbackUnitMeasureId, now);
    }

    public static string? ValidateTierPriceOverlaps(IEnumerable<TierPriceDto> tiers)
    {
        var groups = tiers.GroupBy(t => t.MinQuantity);

        foreach (var group in groups)
        {
            var ordered = group
                .OrderBy(t => t.ValidFrom)
                .ToList();

            for (var i = 0; i < ordered.Count; i++)
            {
                var current = ordered[i];

                if (current.ValidTo.HasValue && current.ValidTo.Value < current.ValidFrom)
                {
                    return $"El precio preferencial con cantidad mínima {current.MinQuantity} tiene ValidTo anterior a ValidFrom.";
                }

                for (var j = i + 1; j < ordered.Count; j++)
                {
                    var next = ordered[j];
                    if (DatesOverlap(current.ValidFrom, current.ValidTo, next.ValidFrom, next.ValidTo))
                    {
                        return $"Los precios preferenciales con cantidad mínima {current.MinQuantity} se traslapan en fechas.";
                    }
                }
            }
        }

        return null;
    }

    public static bool DatesOverlap(DateOnly fromA, DateOnly? toA, DateOnly fromB, DateOnly? toB)
    {
        var endA = toA ?? DateOnly.MaxValue;
        var endB = toB ?? DateOnly.MaxValue;
        return fromA <= endB && fromB <= endA;
    }

    public static void SoftDeleteSupplierProduct(SupplierProduct supplierProduct, DateTime now)
    {
        supplierProduct.IsActive = false;
        supplierProduct.DeletedAt = now;
        CloseActivePriceHistories(supplierProduct, now);

        foreach (var tier in supplierProduct.TierPrices.Where(t => t.DeletedAt == null))
        {
            tier.DeletedAt = now;
            if (!tier.ValidTo.HasValue || tier.ValidTo.Value > DateOnly.FromDateTime(now))
            {
                tier.ValidTo = DateOnly.FromDateTime(now);
            }
        }
    }
}
