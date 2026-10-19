using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Shopping;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;

public static class SupplierProductPriceHelper
{
    public static readonly DateTime ActiveEffectiveTo = DateTime.MaxValue;

    private const string PreferentialOverlapMessage =
        "La vigencia del precio preferencial se cruza con otro acuerdo del mismo vínculo.";

    private const string CurrentEndBeforeTodayMessage =
        "La fecha de fin del precio preferencial vigente no puede ser anterior al día actual.";

    // aqui Crea el vínculo con su precio unitario y, si viene, un preferencial.
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
            AddPreferential(
                link,
                preferential,
                unitMeasureId,
                ResolvePreferentialEffectiveFrom(preferential.ValidFrom!.Value, now));
        }

        return link;
    }

    // Arma la fila del preferencial a partir del body.
    private static SupplierProductTierPrice CreateTierPrice(
        TierPriceDto tier,
        Guid unitMeasureId) =>
        new()
        {
            MinQuantity = tier.MinQuantity!.Value,
            PreferentialPrice = tier.PreferentialPrice!.Value,
            ValidFrom = tier.ValidFrom!.Value,
            ValidTo = tier.ValidTo,
            UnitMeasureId = unitMeasureId
        };

    // Una fecha futura empieza a las 00:00; si es hoy, empieza ahora.
    private static DateTime ResolvePreferentialEffectiveFrom(DateOnly validFrom, DateTime fallbackNow)
    {
        var from = validFrom.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        return from > fallbackNow ? from : fallbackNow;
    }

    // El hasta inclusivo cubre todo el día; sin hasta, el período queda abierto.
    private static DateTime ResolvePreferentialEffectiveTo(DateOnly? validTo) =>
        validTo.HasValue
            ? validTo.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc)
            : ActiveEffectiveTo;

    // Oculta la fecha abierta para que la API devuelva null.
    public static DateTime? ToApiEffectiveTo(DateTime effectiveTo) =>
        effectiveTo == ActiveEffectiveTo || effectiveTo == DateTime.MaxValue
            ? null
            : effectiveTo;

    // Indica si esa ventana del historial está vigente ahora.
    public static bool IsCurrentPrice(DateTime effectiveFrom, DateTime? effectiveTo, DateTime utcNow) =>
        ResolvePriceWindowStatus(effectiveFrom, effectiveTo, utcNow) == PriceWindowStatus.Current;


    // Calcula vigente, programado o histórico según la fecha.
    public static PriceWindowStatus ResolvePriceWindowStatus(
        DateTime effectiveFrom,
        DateTime? effectiveTo,
        DateTime utcNow)
    {
        if (effectiveTo is not null && effectiveTo <= utcNow)
        {
            return PriceWindowStatus.Historical;
        }

        if (effectiveFrom > utcNow)
        {
            return PriceWindowStatus.Scheduled;
        }

        return PriceWindowStatus.Current;
    }

    // Cierra el historial abierto al desactivar el vínculo de supplier-product.
    private static void CloseActivePriceHistories(SupplierProduct supplierProduct, DateTime closedAt)
    {
        foreach (var history in supplierProduct.PriceHistories
                     .Where(h => h.DeletedAt == null && h.EffectiveTo == ActiveEffectiveTo))
        {
            history.EffectiveTo = closedAt;
        }
    }

    // aqui la logica de Cerrar el unitario anterior y abre uno nuevo si cambió precio o moneda del registro.
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

    // Un body nulo no cambia nada; vacío cierra el vigente; uno futuro edita el programado.
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

        var requested = tierPrices[0];
        var dateError = ValidatePreferentialDates(requested, now);
        if (dateError is not null)
        {
            return dateError;
        }

        var today = DateOnly.FromDateTime(now);
        var current = FindCurrentPreferential(supplierProduct, today);
        var scheduled = FindScheduledPreferential(supplierProduct, today);
        var targetsScheduled = requested.ValidFrom is DateOnly validFrom && validFrom > today;

        return targetsScheduled
            ? ApplyScheduledPreferential(supplierProduct, scheduled, requested, fallbackUnitMeasureId, now, today)
            : ApplyCurrentPreferential(supplierProduct, current, requested, fallbackUnitMeasureId, now, today);
    }

    // Exige cantidad, precio y fecha cuando el preferencial es nuevo.
    public static string? MissingNewPreferentialMessage(TierPriceDto? tier)
    {
        if (tier is null)
        {
            return null;
        }

        if (tier.MinQuantity is null || tier.PreferentialPrice is null || tier.ValidFrom is null)
        {
            return "Debe indicar cantidad mínima, precio y fecha de inicio para registrar un precio preferencial.";
        }

        return null;
    }

    // Aplica precio unitario, moneda, unidad y preferencial de un vínculo existente.
    public static string? ApplyExistingLinkUpdate(
        SupplierProduct supplierProduct,
        decimal? unitPrice,
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

    private static string? ValidatePreferentialDates(TierPriceDto tier, DateTime utcNow)
    {
        var today = DateOnly.FromDateTime(utcNow);

        if (tier.ValidFrom is DateOnly validFrom && validFrom < today)
        {
            return "La fecha inicial del precio preferencial no puede ser anterior al día actual.";
        }

        if (tier.ValidTo is DateOnly validTo && tier.ValidFrom is DateOnly from && validTo < from)
        {
            return "La fecha de fin de vigencia no puede ser anterior a la fecha de inicio.";
        }

        return null;
    }

    // En el registro solo se acepta un preferencial y su effectiveTo no puede ir antes del effectiveFrom.
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
            if (tier.ValidTo is DateOnly validTo && tier.ValidFrom is DateOnly validFrom && validTo < validFrom)
            {
                return "El precio preferencial tiene ValidTo anterior a ValidFrom.";
            }
        }

        return null;
    }

    public static bool IsTierCurrentlyValid(DateOnly validFrom, DateOnly? validTo, DateOnly today) =>
        validFrom <= today && (!validTo.HasValue || validTo.Value >= today);


    private static bool IsTierActiveOrUpcoming(DateOnly validFrom, DateOnly? validTo, DateOnly today) =>
        !validTo.HasValue || validTo.Value >= today;


    // Desactiva el vínculo y cierra sus precios abiertos.
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

    // Edita el vigente o lo crea si todavía no hay uno.
    private static string? ApplyCurrentPreferential(
        SupplierProduct supplierProduct,
        SupplierProductTierPrice? current,
        TierPriceDto requested,
        Guid fallbackUnitMeasureId,
        DateTime now,
        DateOnly today)
    {
        if (current is null)
        {
            return AddNewPreferentialWindow(supplierProduct, requested, fallbackUnitMeasureId, now, today);
        }

        var history = FindPreferentialHistory(supplierProduct, current, now, scheduled: false);
        var merged = MergePreferential(supplierProduct, current, requested, history);
        merged.ValidFrom = current.ValidFrom;

        if (merged.ValidTo is DateOnly validTo && validTo < current.ValidFrom)
        {
            return "La fecha de fin de vigencia no puede ser anterior a la fecha de inicio.";
        }

        if (merged.ValidTo is DateOnly end && end < today)
        {
            return CurrentEndBeforeTodayMessage;
        }

        var historyCurrency = history?.Currency ?? supplierProduct.Currency;
        var currencyChanged = merged.Currency is Currency currency && currency != historyCurrency;
        var commercialChanged =
            merged.MinQuantity != current.MinQuantity ||
            merged.PreferentialPrice != current.PreferentialPrice ||
            currencyChanged;
        var endChanged = merged.ValidTo != current.ValidTo;
        var unitMeasureChanged = merged.UnitMeasureId is Guid unitMeasureId && current.UnitMeasureId != unitMeasureId;

        if (!commercialChanged && !endChanged && !unitMeasureChanged)
        {
            return null;
        }

        // Solo cambió el hasta o la unidad: el registro sigue siendo el mismo.
        if (!commercialChanged)
        {
            return UpdateCurrentPreferentialRow(
                supplierProduct,
                current,
                history,
                merged,
                commercialChanged,
                now,
                today);
        }

        // El vigente empezó hoy: el precio nuevo queda en esa misma fila.
        if (current.ValidFrom >= today)
        {
            return UpdateCurrentPreferentialRow(
                supplierProduct,
                current,
                history,
                merged,
                commercialChanged,
                now,
                today);
        }

        var openedOverlap = FindPreferentialOverlap(
            supplierProduct,
            current,
            today,
            merged.ValidTo,
            today);
        if (openedOverlap is not null)
        {
            return openedOverlap;
        }

        var yesterday = today.AddDays(-1);
        current.ValidTo = yesterday;
        CloseCurrentPreferentialHistories(
            supplierProduct,
            yesterday.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc),
            now);

        var openedUnitMeasureId = merged.UnitMeasureId ?? current.UnitMeasureId ?? fallbackUnitMeasureId;
        AddPreferential(
            supplierProduct,
            new TierPriceDto
            {
                MinQuantity = merged.MinQuantity,
                PreferentialPrice = merged.PreferentialPrice,
                ValidFrom = today,
                ValidTo = merged.ValidTo,
                UnitMeasureId = openedUnitMeasureId,
                Currency = merged.Currency ?? historyCurrency
            },
            openedUnitMeasureId,
            today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        supplierProduct.LastPriceUpdate = now;
        return null;
    }

    // Edita el programado o lo crea si todavía no hay uno.
    private static string? ApplyScheduledPreferential(
        SupplierProduct supplierProduct,
        SupplierProductTierPrice? scheduled,
        TierPriceDto requested,
        Guid fallbackUnitMeasureId,
        DateTime now,
        DateOnly today)
    {
        if (scheduled is null)
        {
            return AddNewPreferentialWindow(supplierProduct, requested, fallbackUnitMeasureId, now, today);
        }

        var history = FindPreferentialHistory(supplierProduct, scheduled, now, scheduled: true);
        var merged = MergePreferential(supplierProduct, scheduled, requested, history);
        if (merged.ValidFrom is null)
        {
            return MissingNewPreferentialMessage(merged);
        }

        if (merged.ValidTo is DateOnly validTo && validTo < merged.ValidFrom.Value)
        {
            return "La fecha de fin de vigencia no puede ser anterior a la fecha de inicio.";
        }

        var overlap = FindPreferentialOverlap(
            supplierProduct,
            scheduled,
            merged.ValidFrom.Value,
            merged.ValidTo,
            today);
        if (overlap is not null)
        {
            return overlap;
        }

        scheduled.MinQuantity = merged.MinQuantity ?? scheduled.MinQuantity;
        scheduled.PreferentialPrice = merged.PreferentialPrice ?? scheduled.PreferentialPrice;
        scheduled.ValidFrom = merged.ValidFrom.Value;
        scheduled.ValidTo = merged.ValidTo;
        if (merged.UnitMeasureId is Guid unitMeasureId)
        {
            scheduled.UnitMeasureId = unitMeasureId;
        }

        WritePreferentialHistory(
            supplierProduct,
            history,
            scheduled,
            merged,
            ResolvePreferentialEffectiveFrom(scheduled.ValidFrom, now));
        supplierProduct.LastPriceUpdate = now;
        return null;
    }

    // Valida y agrega un preferencial nuevo sin tocar las otras ventanas fechas.
    private static string? AddNewPreferentialWindow(
        SupplierProduct supplierProduct,
        TierPriceDto requested,
        Guid fallbackUnitMeasureId,
        DateTime now,
        DateOnly today)
    {
        var missing = MissingNewPreferentialMessage(requested);
        if (missing is not null)
        {
            return missing;
        }

        if (requested.ValidTo is DateOnly newEnd && newEnd < today)
        {
            return CurrentEndBeforeTodayMessage;
        }

        var overlap = FindPreferentialOverlap(
            supplierProduct,
            null,
            requested.ValidFrom!.Value,
            requested.ValidTo,
            today);
        if (overlap is not null)
        {
            return overlap;
        }

        AddPreferential(
            supplierProduct,
            requested,
            requested.UnitMeasureId ?? fallbackUnitMeasureId,
            ResolvePreferentialEffectiveFrom(requested.ValidFrom!.Value, now));
        supplierProduct.LastPriceUpdate = now;
        return null;
    }

    // Actualiza la misma fila del vigente.
    private static string? UpdateCurrentPreferentialRow(
        SupplierProduct supplierProduct,
        SupplierProductTierPrice current,
        HistoryPrices? history,
        TierPriceDto merged,
        bool commercialChanged,
        DateTime now,
        DateOnly today)
    {
        var sameRowOverlap = FindPreferentialOverlap(
            supplierProduct,
            current,
            current.ValidFrom,
            merged.ValidTo,
            today);
        if (sameRowOverlap is not null)
        {
            return sameRowOverlap;
        }

        current.ValidTo = merged.ValidTo;
        if (merged.UnitMeasureId is Guid resolvedUnitMeasureId)
        {
            current.UnitMeasureId = resolvedUnitMeasureId;
        }

        if (commercialChanged)
        {
            current.MinQuantity = merged.MinQuantity!.Value;
            current.PreferentialPrice = merged.PreferentialPrice!.Value;
        }

        WritePreferentialHistory(
            supplierProduct,
            history,
            current,
            merged,
            history?.EffectiveFrom ?? ResolvePreferentialEffectiveFrom(current.ValidFrom, now));
        supplierProduct.LastPriceUpdate = now;
        return null;
    }

    // Cierra el vigente ahora y deja el programado como está.
    private static void ClearPreferentialPrices(SupplierProduct supplierProduct, DateTime now)
    {
        var today = DateOnly.FromDateTime(now);
        var currentTiers = supplierProduct.TierPrices
            .Where(tier => tier.DeletedAt == null && IsTierCurrentlyValid(tier.ValidFrom, tier.ValidTo, today))
            .ToList();

        if (currentTiers.Count == 0)
        {
            return;
        }

        var yesterday = today.AddDays(-1);
        CloseCurrentPreferentialHistories(
            supplierProduct,
            yesterday.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc),
            now);

        foreach (var tier in currentTiers)
        {
            if (tier.ValidFrom < today)
            {
                tier.ValidTo = yesterday;
            }
            else
            {
                tier.DeletedAt = now;
                tier.ValidTo = today;
            }
        }

        supplierProduct.LastPriceUpdate = now;
    }

    // Cierra el historial que cubre este momento, sin tocar el programado.
    private static void CloseCurrentPreferentialHistories(
        SupplierProduct supplierProduct,
        DateTime yesterdayEnd,
        DateTime now)
    {
        foreach (var history in supplierProduct.PriceHistories
                     .Where(history =>
                         history.DeletedAt == null &&
                         history.PriceType == SupplierPriceHistoryType.PreferentialPrice &&
                         history.EffectiveFrom <= now &&
                         (history.EffectiveTo == ActiveEffectiveTo || history.EffectiveTo > now))
                     .ToList())
        {
            history.EffectiveTo = history.EffectiveFrom <= yesterdayEnd ? yesterdayEnd : now;
        }
    }

    // Devuelve el preferencial cuya vigencia incluye la fecha del hoy.
    private static SupplierProductTierPrice? FindCurrentPreferential(
        SupplierProduct supplierProduct,
        DateOnly today)
    {
        return supplierProduct.TierPrices
            .Where(tier => tier.DeletedAt == null && IsTierCurrentlyValid(tier.ValidFrom, tier.ValidTo, today))
            .OrderBy(tier => tier.ValidFrom)
            .FirstOrDefault();
    }

    // Devuelve el preferencial que empieza después del dia.
    private static SupplierProductTierPrice? FindScheduledPreferential(
        SupplierProduct supplierProduct,
        DateOnly today)
    {
        return supplierProduct.TierPrices
            .Where(tier =>
                tier.DeletedAt == null &&
                tier.ValidFrom > today &&
                IsTierActiveOrUpcoming(tier.ValidFrom, tier.ValidTo, today))
            .OrderBy(tier => tier.ValidFrom)
            .FirstOrDefault();
    }

    // Completa los campos omitidos con los del acuerdo que se está editando.
    private static TierPriceDto MergePreferential(
        SupplierProduct supplierProduct,
        SupplierProductTierPrice active,
        TierPriceDto requested,
        HistoryPrices? history)
    {
        return new TierPriceDto
        {
            MinQuantity = requested.MinQuantity ?? active.MinQuantity,
            PreferentialPrice = requested.PreferentialPrice ?? active.PreferentialPrice,
            ValidFrom = requested.ValidFrom ?? active.ValidFrom,
            ValidTo = requested.ValidTo ?? active.ValidTo,
            UnitMeasureId = requested.UnitMeasureId ?? active.UnitMeasureId,
            Currency = requested.Currency ?? history?.Currency ?? supplierProduct.Currency
        };
    }

    // Busca el historial abierto del vigente o del programado, según la bandera.
    private static HistoryPrices? FindPreferentialHistory(
        SupplierProduct supplierProduct,
        SupplierProductTierPrice tier,
        DateTime now,
        bool scheduled)
    {
        return supplierProduct.PriceHistories
            .Where(history =>
                history.DeletedAt == null &&
                history.PriceType == SupplierPriceHistoryType.PreferentialPrice &&
                history.Price == tier.PreferentialPrice &&
                history.MinQuantity == tier.MinQuantity &&
                (scheduled ? history.EffectiveFrom > now : history.EffectiveFrom <= now) &&
                (history.EffectiveTo == ActiveEffectiveTo || history.EffectiveTo > now))
            .OrderByDescending(history => history.EffectiveFrom)
            .FirstOrDefault();
    }

    // Rechaza la ventana si pisa otro preferencial vigente o programado.
    private static string? FindPreferentialOverlap(
        SupplierProduct supplierProduct,
        SupplierProductTierPrice? excluded,
        DateOnly start,
        DateOnly? end,
        DateOnly today)
    {
        var overlaps = supplierProduct.TierPrices.Any(tier =>
            tier.DeletedAt == null &&
            !ReferenceEquals(tier, excluded) &&
            IsTierActiveOrUpcoming(tier.ValidFrom, tier.ValidTo, today) &&
            PreferentialWindowsOverlap(start, end, tier.ValidFrom, tier.ValidTo));

        return overlaps ? PreferentialOverlapMessage : null;
    }

    // Dos intervalos se cruzan cuando uno empieza antes de que el otro termine.
    private static bool PreferentialWindowsOverlap(
        DateOnly startA,
        DateOnly? endA,
        DateOnly startB,
        DateOnly? endB)
    {
        var aEnd = endA ?? DateOnly.MaxValue;
        var bEnd = endB ?? DateOnly.MaxValue;
        return startA <= bEnd && startB <= aEnd;
    }

    // Agrega el tier y su historial.
    private static void AddPreferential(
        SupplierProduct supplierProduct,
        TierPriceDto tier,
        Guid unitMeasureId,
        DateTime effectiveFrom)
    {
        supplierProduct.TierPrices.Add(CreateTierPrice(tier, unitMeasureId));
        supplierProduct.PriceHistories.Add(new HistoryPrices
        {
            PriceType = SupplierPriceHistoryType.PreferentialPrice,
            Price = tier.PreferentialPrice!.Value,
            MinQuantity = tier.MinQuantity,
            Currency = tier.Currency ?? supplierProduct.Currency,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = ResolvePreferentialEffectiveTo(tier.ValidTo)
        });
    }

    // Actualiza el historial abierto o crea uno si esa ventana no tenía.
    private static void WritePreferentialHistory(
        SupplierProduct supplierProduct,
        HistoryPrices? history,
        SupplierProductTierPrice tier,
        TierPriceDto merged,
        DateTime effectiveFrom)
    {
        if (history is null)
        {
            supplierProduct.PriceHistories.Add(new HistoryPrices
            {
                PriceType = SupplierPriceHistoryType.PreferentialPrice,
                Price = tier.PreferentialPrice,
                MinQuantity = tier.MinQuantity,
                Currency = merged.Currency ?? supplierProduct.Currency,
                EffectiveFrom = effectiveFrom,
                EffectiveTo = ResolvePreferentialEffectiveTo(tier.ValidTo)
            });
            return;
        }

        history.Price = tier.PreferentialPrice;
        history.MinQuantity = tier.MinQuantity;
        history.Currency = merged.Currency ?? history.Currency;
        history.EffectiveFrom = effectiveFrom;
        history.EffectiveTo = ResolvePreferentialEffectiveTo(tier.ValidTo);
    }

    // Toma el único preferencial enviado al crear el vínculo.
    private static TierPriceDto? NormalizeSinglePreferential(IEnumerable<TierPriceDto>? tierPrices)
    {
        if (tierPrices is null)
        {
            return null;
        }

        var list = tierPrices.ToList();
        return list.Count == 0 ? null : list[0];
    }
}
