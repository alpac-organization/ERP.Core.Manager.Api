using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Shopping;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class UpdateSupplierInformationHandler(
        IUnitOfWork _unitOfWork,
        ILogger<UpdateSupplierInformationHandler> _logger,
        IErrorManager _errorManager)
        : BaseValidatorHandler<UpdateSupplierInformationCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(UpdateSupplierInformationCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse;
            }

            if (access.Role?.RoleType == RoleType.Supervisor)
            {
                _logger.LogWarning(
                    "Usuario {UserId} (Supervisor) intentó actualizar proveedor {SupplierId} sin permiso",
                    request.UserId,
                    request.SupplierId);
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para registrar un proveedor", "ERP:01");
            }

            var user = await _unitOfWork.Users.Entities
                .Where(u => u.Id == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Usuario desconocido!", "ERP:01");
            }

            if (access.Role!.RoleType != RoleType.Administrator && access.Role.RoleType != RoleType.Manager)
            {
                _logger.LogWarning(
                    "Usuario {UserId} con rol {RoleType} intentó actualizar proveedor {SupplierId} sin permiso",
                    request.UserId,
                    access.Role.RoleType,
                    request.SupplierId);
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para actualizar este proveedor", "ERP:01");
            }

            var supplier = await _unitOfWork.Suppliers.Entities
                .AsSplitQuery()
                .Include(sup => sup.SupplierPaymentMethods.Where(pm => pm.DeletedAt == null))
                .Include(sup => sup.SupplierProducts.Where(sp => sp.DeletedAt == null))
                    .ThenInclude(sp => sp.TierPrices)
                .Include(sup => sup.SupplierProducts.Where(sp => sp.DeletedAt == null))
                    .ThenInclude(sp => sp.PriceHistories)
                .Include(sup => sup.SupplierProducts.Where(sp => sp.DeletedAt == null))
                    .ThenInclude(sp => sp.Product)
                .FirstOrDefaultAsync(sup => sup.Id == request.SupplierId, cancellationToken);

            if (supplier is null)
            {
                return _errorManager.ThrowBadRequest<bool>("El registro de este proveedor no existe", "ERP:04");
            }

            supplier.SuppliersLegalName = request.SuppliersLegalName ?? supplier.SuppliersLegalName;
            supplier.CommercialName = request.CommercialName ?? supplier.CommercialName;
            supplier.IdentificationNumber = request.IdentificationNumber ?? supplier.IdentificationNumber;
            supplier.ConstitutionType = request.ConstitutionType ?? supplier.ConstitutionType;
            supplier.IdentificationType = request.IdentificationType ?? supplier.IdentificationType;

            if (request.SupplierDetails is not null)
            {
                var supplierDetails = await _unitOfWork.SuppliersDetails.Entities
                    .Where(supl => supl.SupplierId == supplier.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (supplierDetails is not null)
                {
                    supplierDetails.HasCredit = request.SupplierDetails.HasCredit ?? supplierDetails.HasCredit;
                    supplierDetails.CreditDays = request.SupplierDetails.CreditDays ?? supplierDetails.CreditDays;
                    supplierDetails.Address = request.SupplierDetails.Address ?? supplierDetails.Address;
                    supplierDetails.EmailSupport = request.SupplierDetails.EmailSupport ?? supplierDetails.EmailSupport;
                    supplierDetails.ContactName = request.SupplierDetails.ContactName ?? supplierDetails.ContactName;
                    supplierDetails.ContactEmail = request.SupplierDetails.ContactEmail ?? supplierDetails.ContactEmail;
                    supplierDetails.ContactPhoneNumber = request.SupplierDetails.ContactPhoneNumber ?? supplierDetails.ContactPhoneNumber;
                    supplierDetails.ExclusiveBrandsOrParts = request.SupplierDetails.ExclusiveBrandsOrParts ?? supplierDetails.ExclusiveBrandsOrParts;
                    supplierDetails.CreditLimit = request.SupplierDetails.CreditLimit ?? supplierDetails.CreditLimit;
                    supplierDetails.CreditCurrency = request.SupplierDetails.CreditCurrency ?? supplierDetails.CreditCurrency;
                    supplierDetails.AlertDaysBeforeDue = request.SupplierDetails.AlertDaysBeforeDue ?? supplierDetails.AlertDaysBeforeDue;
                    supplierDetails.ApplyIrRetention = request.SupplierDetails.ApplyIrRetention ?? supplierDetails.ApplyIrRetention;
                    supplierDetails.ApplyMunicipalRetention = request.SupplierDetails.ApplyMunicipalRetention ?? supplierDetails.ApplyMunicipalRetention;
                    supplierDetails.IsTaxExempt = request.SupplierDetails.IsTaxExempt ?? supplierDetails.IsTaxExempt;
                }
            }

            if (request.PaymentMethods is not null)
            {
                SyncPaymentMethods(supplier, request.PaymentMethods);
            }

            if (request.Products is not null)
            {
                var syncError = await SyncProductsAsync(supplier, request.Products, cancellationToken);
                if (syncError is not null)
                {
                    return _errorManager.ThrowBadRequest<bool>(syncError, "ERP:SUPPLIER_PRODUCTS");
                }
            }

            await _unitOfWork.Suppliers.UpdateAsync(supplier);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Usuario {UserId} actualizó el proveedor {SupplierId}",
                request.UserId,
                request.SupplierId);

            return true;
        }

        private static void SyncPaymentMethods(Supplier supplier, List<PaymentMethodType> paymentMethods)
        {
            var now = DateTime.UtcNow;
            var distinctMethods = paymentMethods.Distinct().ToList();

            foreach (var existing in supplier.SupplierPaymentMethods.Where(pm => pm.DeletedAt == null))
            {
                existing.IsActive = false;
                existing.DeletedAt = now;
            }

            foreach (var method in distinctMethods)
            {
                supplier.SupplierPaymentMethods.Add(new SupplierPaymentMethod
                {
                    Id = Guid.NewGuid(),
                    SupplierId = supplier.Id,
                    PaymentMethodType = method,
                    IsActive = true
                });
            }
        }

        private async Task<string?> SyncProductsAsync(
            Supplier supplier,
            List<SupplierProductItemDto> products,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var productIds = products.Select(p => p.ProductId).ToList();

            if (productIds.Distinct().Count() != productIds.Count)
            {
                return "No se puede relacionar el mismo producto más de una vez al proveedor.";
            }

            if (productIds.Count > 0)
            {
                var existingProductCount = await _unitOfWork.Products.Entities
                    .CountAsync(p => productIds.Contains(p.Id) && p.DeletedAt == null, cancellationToken);

                if (existingProductCount != productIds.Count)
                {
                    return "Uno o más productos seleccionados no existen.";
                }
            }

            var activeLinks = supplier.SupplierProducts
                .Where(sp => sp.IsActive && sp.DeletedAt == null)
                .ToList();

            var incomingByProductId = products.ToDictionary(p => p.ProductId);

            foreach (var link in activeLinks.Where(sp => !incomingByProductId.ContainsKey(sp.ProductId)))
            {
                SupplierProductPriceHelper.SoftDeleteSupplierProduct(link, now);
            }

            foreach (var item in products)
            {
                var overlapError = SupplierProductPriceHelper.ValidateTierPriceOverlaps(item.TierPrices ?? []);
                if (overlapError is not null)
                {
                    return overlapError;
                }

                var existingLink = activeLinks.FirstOrDefault(sp => sp.ProductId == item.ProductId);

                if (existingLink is null)
                {
                    supplier.SupplierProducts.Add(
                        SupplierProductPriceHelper.BuildSupplierProduct(
                            supplier.Id,
                            item.ProductId,
                            item.UnitPrice,
                            item.Currency,
                            item.TierPrices,
                            unitMeasureId: null,
                            now));
                    continue;
                }

                SupplierProductPriceHelper.ApplyUnitPriceChange(existingLink, item.UnitPrice, now);
                existingLink.Currency = item.Currency;

                if (item.TierPrices is not null)
                {
                    var unitMeasureId = existingLink.Product?.UnitMeasureId;
                    var syncError = SupplierProductPriceHelper.SyncTierPrices(
                        existingLink,
                        item.TierPrices,
                        unitMeasureId,
                        now);

                    if (syncError is not null)
                    {
                        return syncError;
                    }
                }
            }

            _logger.LogInformation(
                "Sincronizados {ProductCount} productos del proveedor {SupplierId}",
                products.Count,
                supplier.Id);

            return null;
        }
    }
}
