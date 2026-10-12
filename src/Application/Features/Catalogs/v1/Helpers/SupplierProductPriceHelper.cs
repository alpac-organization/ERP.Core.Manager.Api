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
        Guid unitMeasureId,
        DateTime now,
        Currency? currency = null)
    {
        var resolvedCurrency = currency ?? Currency.NIO;

        var link = new SupplierProduct
        {
            IsActive = true,
            SupplierId = supplierId,
            UnitPrice = unitPrice,
            LastPriceUpdate = now,
            Currency = resolvedCurrency,
            UnitMeasureId = unitMeasureId,
            PriceHistories =
            [
                new HistoryPrices
                {
                    PriceType = SupplierPriceHistoryType.UnitPrice,
                    Price = unitPrice,
                    Currency = resolvedCurrency,
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

        var preferential = NormalizeSinglePreferential(tierPrices);
        if (preferential is not null)
        {
            ApplyPreferentialReplacement(link, preferential, unitMeasureId, now, isInitial: true);
        }

        return link;
    }

    public static SupplierProductTierPrice CreateTierPrice(
        TierPriceDto tier,
        Guid unitMeasureId) =>
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

    public static void ApplyUnitPriceOrCurrencyChange(
        SupplierProduct supplierProduct,
        decimal? newUnitPrice,
        Currency? newCurrency,
        DateTime now)
    {
        var priceChanged = newUnitPrice.HasValue && supplierProduct.UnitPrice != newUnitPrice.Value;
        var currencyChanged = newCurrency.HasValue && supplierProduct.Currency != newCurrency.Value;

        if (!priceChanged && !currencyChanged)
        {
            return;
        }

        var resultingPrice = newUnitPrice ?? supplierProduct.UnitPrice;
        var resultingCurrency = newCurrency ?? supplierProduct.Currency;

        foreach (var active in supplierProduct.PriceHistories
                     .Where(h =>
                         h.DeletedAt == null &&
                         h.PriceType == SupplierPriceHistoryType.UnitPrice &&
                         h.EffectiveTo == ActiveEffectiveTo)
                     .ToList())
        {
            active.EffectiveTo = now;
        }

        supplierProduct.PriceHistories.Add(new HistoryPrices
        {
            PriceType = SupplierPriceHistoryType.UnitPrice,
            Price = resultingPrice,
            Currency = resultingCurrency,
            EffectiveFrom = now,
            EffectiveTo = ActiveEffectiveTo
        });

        if (priceChanged)
        {
            supplierProduct.UnitPrice = resultingPrice;
        }

        if (currencyChanged)
        {
            supplierProduct.Currency = resultingCurrency;
        }

        supplierProduct.LastPriceUpdate = now;
    }

    public static void ApplyUnitPriceChange(SupplierProduct supplierProduct, decimal newUnitPrice, DateTime now) =>
        ApplyUnitPriceOrCurrencyChange(supplierProduct, newUnitPrice, null, now);

    public static string? ApplyPreferentialPrices(
        SupplierProduct supplierProduct,
        IReadOnlyList<TierPriceDto>? tierPrices,
        Guid fallbackUnitMeasureId,
        DateTime now)
    {
        if (tierPrices is null)
        {
            return null;
        }

        if (tierPrices.Count == 0)
        {
            ClearPreferentialPrices(supplierProduct, now);
            return null;
        }

        if (tierPrices.Count > 1)
        {
            return "Solo se permite un precio preferencial por vínculo proveedor-producto.";
        }

        var tier = tierPrices[0];
        var dateError = ValidatePreferentialDates(tier, now);
        if (dateError is not null)
        {
            return dateError;
        }

        ApplyPreferentialReplacement(supplierProduct, tier, fallbackUnitMeasureId, now, isInitial: false);
        supplierProduct.LastPriceUpdate = now;
        return null;
    }

    public static string? ApplyTierPrices(
        SupplierProduct supplierProduct,
        IReadOnlyList<TierPriceDto> tierPrices,
        Guid fallbackUnitMeasureId,
        DateTime now) =>
        ApplyPreferentialPrices(supplierProduct, tierPrices, fallbackUnitMeasureId, now);

    public static string? ApplyExistingLinkUpdate(
        SupplierProduct supplierProduct,
        decimal unitPrice,
        IReadOnlyList<TierPriceDto>? tierPrices,
        Guid? unitMeasureId,
        Guid fallbackUnitMeasureId,
        DateTime now,
        Currency? currency = null)
    {
        ApplyUnitPriceOrCurrencyChange(supplierProduct, unitPrice, currency, now);

        if (unitMeasureId.HasValue && supplierProduct.UnitMeasureId != unitMeasureId.Value)
        {
            supplierProduct.UnitMeasureId = unitMeasureId.Value;
        }

        var tierFallback = unitMeasureId
            ?? (supplierProduct.UnitMeasureId != Guid.Empty
                ? supplierProduct.UnitMeasureId
                : fallbackUnitMeasureId);

        return ApplyPreferentialPrices(supplierProduct, tierPrices, tierFallback, now);
    }

    public static Currency ResolveLinkCurrency(
        Currency? linkCurrency,
        Currency? supplierDetailsCurrency) =>
        linkCurrency
        ?? supplierDetailsCurrency
        ?? Currency.NIO;

    public static string? ValidatePreferentialDates(TierPriceDto tier, DateTime utcNow)
    {
        var today = DateOnly.FromDateTime(utcNow);

        if (tier.ValidFrom < today)
        {
            return "La fecha inicial del precio preferencial no puede ser anterior al día actual.";
        }

        if (tier.ValidTo.HasValue && tier.ValidTo.Value < tier.ValidFrom)
        {
            return "La fecha de fin de vigencia no puede ser anterior a la fecha de inicio.";
        }

        return null;
    }

    public static string? ValidateTierPriceOverlaps(IEnumerable<TierPriceDto> tiers)
    {
        var list = tiers.ToList();
        if (list.Count > 1)
        {
            return "Solo se permite un precio preferencial por vínculo proveedor-producto.";
        }

        if (list.Count == 1)
        {
            var tier = list[0];
            if (tier.ValidTo.HasValue && tier.ValidTo.Value < tier.ValidFrom)
            {
                return "El precio preferencial tiene ValidTo anterior a ValidFrom.";
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

    public static bool IsTierCurrentlyValid(DateOnly validFrom, DateOnly? validTo, DateOnly today) =>
        validFrom <= today && (!validTo.HasValue || validTo.Value >= today);

    public static bool IsTierActiveOrUpcoming(DateOnly validFrom, DateOnly? validTo, DateOnly today) =>
        !validTo.HasValue || validTo.Value >= today;

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

    public static void ClearPreferentialPrices(SupplierProduct supplierProduct, DateTime now)
    {
        foreach (var history in supplierProduct.PriceHistories
                     .Where(h =>
                         h.DeletedAt == null &&
                         h.PriceType == SupplierPriceHistoryType.PreferentialPrice &&
                         (h.EffectiveTo == ActiveEffectiveTo || h.EffectiveTo > now))
                     .ToList())
        {
            if (history.EffectiveTo == ActiveEffectiveTo || history.EffectiveTo > now)
            {
                history.EffectiveTo = now;
            }
        }

        var today = DateOnly.FromDateTime(now);
        foreach (var tier in supplierProduct.TierPrices.Where(t => t.DeletedAt == null))
        {
            tier.DeletedAt = now;
            if (!tier.ValidTo.HasValue || tier.ValidTo.Value > today)
            {
                tier.ValidTo = today;
            }
        }

        supplierProduct.LastPriceUpdate = now;
    }

    private static TierPriceDto? NormalizeSinglePreferential(IEnumerable<TierPriceDto>? tierPrices)
    {
        if (tierPrices is null)
        {
            return null;
        }

        var list = tierPrices.ToList();
        return list.Count == 0 ? null : list[0];
    }

    private static void ApplyPreferentialReplacement(
        SupplierProduct supplierProduct,
        TierPriceDto tier,
        Guid fallbackUnitMeasureId,
        DateTime now,
        bool isInitial)
    {
        var newEffectiveFrom = ResolvePreferentialEffectiveFrom(tier.ValidFrom, now);
        var newEffectiveTo = ResolvePreferentialEffectiveTo(tier.ValidTo);
        var preferentialCurrency = tier.Currency ?? supplierProduct.Currency;
        var resolvedUnitMeasureId = tier.UnitMeasureId ?? fallbackUnitMeasureId;

        var historyCloseAt = newEffectiveFrom > now ? newEffectiveFrom : now;

        if (!isInitial)
        {
            foreach (var history in supplierProduct.PriceHistories
                         .Where(h =>
                             h.DeletedAt == null &&
                             h.PriceType == SupplierPriceHistoryType.PreferentialPrice &&
                             (h.EffectiveTo == ActiveEffectiveTo || h.EffectiveTo > historyCloseAt))
                         .ToList())
            {
                history.EffectiveTo = historyCloseAt;
            }

            var today = DateOnly.FromDateTime(now);
            var dayBeforeNew = DateOnly.FromDateTime(newEffectiveFrom).AddDays(-1);

            foreach (var existing in supplierProduct.TierPrices.Where(t => t.DeletedAt == null).ToList())
            {
                if (newEffectiveFrom > now)
                {
                    existing.ValidTo = dayBeforeNew < existing.ValidFrom
                        ? existing.ValidFrom
                        : dayBeforeNew;

                    if (existing.ValidTo < today)
                    {
                        existing.DeletedAt = now;
                    }
                }
                else
                {
                    existing.DeletedAt = now;
                    if (!existing.ValidTo.HasValue || existing.ValidTo.Value > today)
                    {
                        existing.ValidTo = today;
                    }
                }
            }
        }

        supplierProduct.TierPrices.Add(CreateTierPrice(tier, resolvedUnitMeasureId));
        supplierProduct.PriceHistories.Add(new HistoryPrices
        {
            PriceType = SupplierPriceHistoryType.PreferentialPrice,
            Price = tier.PreferentialPrice,
            MinQuantity = tier.MinQuantity,
            Currency = preferentialCurrency,
            EffectiveFrom = newEffectiveFrom,
            EffectiveTo = newEffectiveTo
        });
    }
}
