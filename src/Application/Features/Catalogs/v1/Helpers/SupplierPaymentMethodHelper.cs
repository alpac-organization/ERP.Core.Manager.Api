using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Shopping;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;

public static class SupplierPaymentMethodHelper
{
    public static void SyncPaymentMethods(
        Supplier supplier,
        IReadOnlyList<SupplierPaymentMethod> storedMethods,
        IReadOnlyList<PaymentMethodType> requestedMethods,
        DateTime now)
    {
        var desired = requestedMethods.Distinct().ToHashSet();

        foreach (var existing in storedMethods)
        {
            if (desired.Contains(existing.PaymentMethodType))
            {
                if (!existing.IsActive)
                {
                    existing.IsActive = true;
                }

                if (existing.DeletedAt != null)
                {
                    existing.DeletedAt = null;
                }

                desired.Remove(existing.PaymentMethodType);
            }
            else if (existing.IsActive || existing.DeletedAt == null)
            {
                if (existing.IsActive)
                {
                    existing.IsActive = false;
                }

                if (existing.DeletedAt == null)
                {
                    existing.DeletedAt = now;
                }
            }
        }

        supplier.SupplierPaymentMethods ??= [];

        foreach (var paymentMethod in desired)
        {
            supplier.SupplierPaymentMethods.Add(new SupplierPaymentMethod
            {
                SupplierId = supplier.Id,
                PaymentMethodType = paymentMethod,
                IsActive = true
            });
        }
    }

    public static IEnumerable<SupplierPaymentMethod> ActivePaymentMethods(
        IEnumerable<SupplierPaymentMethod> paymentMethods) =>
        paymentMethods.Where(pm => pm.IsActive && pm.DeletedAt == null);
}
