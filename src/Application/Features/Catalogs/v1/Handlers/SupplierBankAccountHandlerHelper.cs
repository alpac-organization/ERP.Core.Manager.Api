using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Entities.Shopping;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    internal static class SupplierBankAccountHandlerHelper
    {
        public static async Task<(bool IsSuccess, Supplier? Supplier, SupplierBankAccount? Account, bool ErrorResult)> FindSupplierAndAccountAsync(
            IUnitOfWork unitOfWork,
            IErrorManager errorManager,
            Guid supplierId,
            Guid bankAccountId,
            CancellationToken cancellationToken)
        {
            var supplier = await unitOfWork.Suppliers.Entities
                .Include(s => s.SupplierBankAccounts)
                .Where(s => s.IsActive && s.Id == supplierId)
                .FirstOrDefaultAsync(cancellationToken);

            if (supplier is null)
            {
                return (false, null, null, errorManager.ThrowBadRequest<bool>("El proveedor especificado no existe", "ERP:NOT_FOUND"));
            }

            var account = supplier.SupplierBankAccounts
                .FirstOrDefault(b => b.Id == bankAccountId && b.DeletedAt == null);

            if (account is null)
            {
                return (false, supplier, null, errorManager.ThrowBadRequest<bool>("La cuenta bancaria especificada no existe", "ERP:NOT_FOUND"));
            }

            return (true, supplier, account, false);
        }
    }
}
