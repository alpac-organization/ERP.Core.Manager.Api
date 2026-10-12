using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Shopping;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;

public static class SupplierPaymentMethodHelper
{
    public static void SyncPaymentMethods(
        Supplier supplier,
        IReadOnlyList<PaymentMethodType> paymentMethods,
        DateTime now)
    {
        var desired = paymentMethods.Distinct().ToHashSet();

        foreach (var existing in supplier.SupplierPaymentMethods.ToList())
        {
            if (desired.Contains(existing.PaymentMethodType))
            {
                existing.IsActive = true;
                existing.DeletedAt = null;
                desired.Remove(existing.PaymentMethodType);
            }
            else if (existing.DeletedAt == null || existing.IsActive)
            {
                existing.IsActive = false;
                existing.DeletedAt = now;
            }
        }

        foreach (var paymentMethod in desired)
        {
            supplier.SupplierPaymentMethods.Add(new SupplierPaymentMethod
            {
                Id = Guid.NewGuid(),
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
