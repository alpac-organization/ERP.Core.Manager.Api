using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Shopping;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;

using SupplierDetailsEntity = ERP.Core.Database.Domain.Entities.Shopping.SupplierDetails;

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
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para actualizar un proveedor", "ERP:01");
            }

            if (access.Role!.RoleType != RoleType.Administrator && access.Role.RoleType != RoleType.Manager)
            {
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para actualizar este proveedor", "ERP:01");
            }

            var supplier = await _unitOfWork.Suppliers.Entities
                .FirstOrDefaultAsync(sup => sup.Id == request.SupplierId, cancellationToken);

            if (supplier is null)
            {
                return _errorManager.ThrowBadRequest<bool>("El registro de este proveedor no existe", "ERP:04");
            }

            var now = DateTime.UtcNow;
            var detailsRequested = HasDetailUpdate(request.SupplierDetails);
            var paymentsRequested = request.PaymentMethods is not null;
            var productsRequested = request.Products is { Count: > 0 };

            PatchSupplierRoot(supplier, request);

            SupplierDetailsEntity? supplierDetails = null;
            if (detailsRequested || paymentsRequested)
            {
                supplierDetails = await _unitOfWork.SuppliersDetails.Entities
                    .FirstOrDefaultAsync(detail => detail.SupplierId == supplier.Id, cancellationToken);
            }

            if (detailsRequested)
            {
                var detailsError = PatchSupplierDetails(supplierDetails, request.SupplierDetails);
                if (detailsError is bool detailsResult)
                {
                    return detailsResult;
                }
            }

            if (paymentsRequested || request.SupplierDetails?.PreferredPaymentMethod is not null)
            {
                var paymentError = await PatchPaymentMethodsAsync(supplier, supplierDetails, request, now, cancellationToken);
                if (paymentError is bool paymentResult)
                {
                    return paymentResult;
                }
            }

            if (productsRequested)
            {
                var productError = await PatchProductsAsync(supplier, supplierDetails, request, now, cancellationToken);
                if (productError is bool productResult)
                {
                    return productResult;
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Usuario {UserId} actualizó el proveedor {SupplierId}", request.UserId, request.SupplierId);

            return true;
        }

        private static void PatchSupplierRoot(Supplier supplier, UpdateSupplierInformationCommand request)
        {
            if (request.SuppliersLegalName is not null && supplier.SuppliersLegalName != request.SuppliersLegalName)
            {
                supplier.SuppliersLegalName = request.SuppliersLegalName;
            }

            if (request.CommercialName is not null && supplier.CommercialName != request.CommercialName)
            {
                supplier.CommercialName = request.CommercialName;
            }

            if (request.IdentificationNumber is not null && supplier.IdentificationNumber != request.IdentificationNumber)
            {
                supplier.IdentificationNumber = request.IdentificationNumber;
            }

            if (request.ConstitutionType.HasValue && supplier.ConstitutionType != request.ConstitutionType.Value)
            {
                supplier.ConstitutionType = request.ConstitutionType.Value;
            }

            if (request.IdentificationType.HasValue && supplier.IdentificationType != request.IdentificationType.Value)
            {
                supplier.IdentificationType = request.IdentificationType.Value;
            }
        }

        private bool? PatchSupplierDetails(SupplierDetailsEntity? supplierDetails, SupplierDetailsInformation details)
        {
            if (supplierDetails is null)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    "El proveedor no tiene detalles registrados. No se pueden actualizar los detalles del proveedor.",
                    "ERP:ERROR_UPDATE");
            }

            if (details.HasCredit.HasValue && supplierDetails.HasCredit != details.HasCredit.Value)
            {
                supplierDetails.HasCredit = details.HasCredit.Value;
            }

            if (details.CreditDays.HasValue && supplierDetails.CreditDays != details.CreditDays.Value)
            {
                supplierDetails.CreditDays = details.CreditDays.Value;
            }

            if (details.Address is not null && supplierDetails.Address != details.Address)
            {
                supplierDetails.Address = details.Address;
            }

            if (details.EmailSupport is not null && supplierDetails.EmailSupport != details.EmailSupport)
            {
                supplierDetails.EmailSupport = details.EmailSupport;
            }

            if (details.ContactName is not null && supplierDetails.ContactName != details.ContactName)
            {
                supplierDetails.ContactName = details.ContactName;
            }

            if (details.ContactEmail is not null && supplierDetails.ContactEmail != details.ContactEmail)
            {
                supplierDetails.ContactEmail = details.ContactEmail;
            }

            if (details.ContactPhoneNumber is not null && supplierDetails.ContactPhoneNumber != details.ContactPhoneNumber)
            {
                supplierDetails.ContactPhoneNumber = details.ContactPhoneNumber;
            }

            if (details.ExclusiveBrandsOrParts is not null && supplierDetails.ExclusiveBrandsOrParts != details.ExclusiveBrandsOrParts)
            {
                supplierDetails.ExclusiveBrandsOrParts = details.ExclusiveBrandsOrParts;
            }

            if (details.ExclusiveStatusComments is not null && supplierDetails.ExclusiveStatusComments != details.ExclusiveStatusComments)
            {
                supplierDetails.ExclusiveStatusComments = details.ExclusiveStatusComments;
            }

            if (details.SupplierType.HasValue && supplierDetails.SupplierType != details.SupplierType.Value)
            {
                supplierDetails.SupplierType = details.SupplierType.Value;
            }

            if (details.Currency.HasValue && supplierDetails.Currency != details.Currency.Value)
            {
                supplierDetails.Currency = details.Currency.Value;
            }

            if (details.CreditLimit.HasValue && supplierDetails.CreditLimit != details.CreditLimit.Value)
            {
                supplierDetails.CreditLimit = details.CreditLimit.Value;
            }

            if (details.CreditCurrency.HasValue && supplierDetails.CreditCurrency != details.CreditCurrency.Value)
            {
                supplierDetails.CreditCurrency = details.CreditCurrency.Value;
            }

            if (details.AlertDaysBeforeDue.HasValue && supplierDetails.AlertDaysBeforeDue != details.AlertDaysBeforeDue.Value)
            {
                supplierDetails.AlertDaysBeforeDue = details.AlertDaysBeforeDue.Value;
            }

            if (details.PreferredPaymentMethod.HasValue && supplierDetails.PreferredPaymentMethod != details.PreferredPaymentMethod.Value)
            {
                supplierDetails.PreferredPaymentMethod = details.PreferredPaymentMethod.Value;
            }

            if (details.ApplyIrRetention.HasValue && supplierDetails.ApplyIrRetention != details.ApplyIrRetention.Value)
            {
                supplierDetails.ApplyIrRetention = details.ApplyIrRetention.Value;
            }

            if (details.ApplyMunicipalRetention.HasValue && supplierDetails.ApplyMunicipalRetention != details.ApplyMunicipalRetention.Value)
            {
                supplierDetails.ApplyMunicipalRetention = details.ApplyMunicipalRetention.Value;
            }

            if (details.IsTaxExempt.HasValue && supplierDetails.IsTaxExempt != details.IsTaxExempt.Value)
            {
                supplierDetails.IsTaxExempt = details.IsTaxExempt.Value;
            }

            return null;
        }

        private async Task<bool?> PatchPaymentMethodsAsync(
            Supplier supplier,
            SupplierDetailsEntity? supplierDetails,
            UpdateSupplierInformationCommand request,
            DateTime now,
            CancellationToken cancellationToken)
        {
            List<SupplierPaymentMethod> storedMethods = [];

            if (request.PaymentMethods is not null)
            {
                storedMethods = await _unitOfWork.Suppliers.Entities
                    .IgnoreQueryFilters()
                    .Where(item => item.Id == supplier.Id)
                    .SelectMany(item => item.SupplierPaymentMethods)
                    .ToListAsync(cancellationToken);
            }

            PaymentMethodType? preferred = request.SupplierDetails?.PreferredPaymentMethod
                ?? (supplierDetails is not null && Enum.IsDefined(supplierDetails.PreferredPaymentMethod)
                    ? supplierDetails.PreferredPaymentMethod
                    : null);

            if (preferred is { } preferredMethod)
            {
                var enabledMethods = request.PaymentMethods
                    ?? await ActivePaymentMethodTypesAsync(supplier.Id, cancellationToken);

                if (!enabledMethods.Contains(preferredMethod))
                {
                    return _errorManager.ThrowBadRequest<bool>(
                        "El método de pago preferido debe estar incluido en los métodos de pago habilitados.",
                        "ERP:ERROR_UPDATE");
                }
            }

            if (request.PaymentMethods is not null)
            {
                SupplierPaymentMethodHelper.SyncPaymentMethods(supplier, storedMethods, request.PaymentMethods, now);
            }

            return null;
        }

        private async Task<List<PaymentMethodType>> ActivePaymentMethodTypesAsync(Guid supplierId, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Suppliers.Entities
                .AsNoTracking()
                .Where(item => item.Id == supplierId)
                .SelectMany(item => item.SupplierPaymentMethods)
                .Where(method => method.IsActive && method.DeletedAt == null)
                .Select(method => method.PaymentMethodType)
                .ToListAsync(cancellationToken);
        }

        private async Task<bool?> PatchProductsAsync(
            Supplier supplier,
            SupplierDetailsEntity? supplierDetails,
            UpdateSupplierInformationCommand request,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var (linkError, productUnitMeasures) = await SupplierProductLinkValidator.ValidateProductsToLinkAsync(
                _unitOfWork,
                request.Products,
                alreadyLinkedProductIds: null,
                "ERP:ERROR_UPDATE",
                cancellationToken);

            if (linkError is not null)
            {
                return _errorManager.ThrowBadRequest<bool>(linkError.Message, linkError.Code);
            }

            var supplierDetailsCurrency = request.SupplierDetails?.Currency
                ?? supplierDetails?.Currency
                ?? await _unitOfWork.SuppliersDetails.Entities
                    .AsNoTracking()
                    .Where(detail => detail.SupplierId == supplier.Id)
                    .Select(detail => (Currency?)detail.Currency)
                    .FirstOrDefaultAsync(cancellationToken)
                ?? Currency.NIO;

            var productIds = request.Products.Select(product => product.ProductId).Distinct().ToList();

            var storedProductLinks = await _unitOfWork.Suppliers.Entities
                .IgnoreQueryFilters()
                .Where(item => item.Id == supplier.Id)
                .SelectMany(item => item.SupplierProducts)
                .Where(link => productIds.Contains(link.ProductId))
                .Include(link => link.PriceHistories)
                .Include(link => link.TierPrices)
                .ToListAsync(cancellationToken);

            foreach (var productItem in request.Products)
            {
                var existingLink = storedProductLinks.FirstOrDefault(link => link.ProductId == productItem.ProductId);
                var productUnitMeasureId = productUnitMeasures[productItem.ProductId];

                if (existingLink is not null)
                {
                    if (!existingLink.IsActive)
                    {
                        existingLink.IsActive = true;
                    }

                    if (existingLink.DeletedAt != null)
                    {
                        existingLink.DeletedAt = null;
                    }

                    var applyError = SupplierProductPriceHelper.ApplyExistingLinkUpdate(
                        existingLink,
                        productItem.UnitPrice,
                        productItem.TierPrices,
                        productItem.UnitMeasureId,
                        productUnitMeasureId,
                        now,
                        productItem.Currency);

                    if (applyError is not null)
                    {
                        return _errorManager.ThrowBadRequest<bool>(applyError, "ERP:ERROR_UPDATE");
                    }

                    continue;
                }

                if (!productItem.UnitPrice.HasValue)
                {
                    return _errorManager.ThrowBadRequest<bool>(
                        "El precio unitario es obligatorio al vincular un producto nuevo.",
                        "ERP:ERROR_UPDATE");
                }

                var missingPreferential = SupplierProductPriceHelper.MissingNewPreferentialMessage(
                    productItem.TierPrices?.FirstOrDefault());
                if (missingPreferential is not null)
                {
                    return _errorManager.ThrowBadRequest<bool>(missingPreferential, "ERP:ERROR_UPDATE");
                }

                supplier.SupplierProducts ??= [];
                supplier.SupplierProducts.Add(
                    SupplierProductPriceHelper.BuildSupplierProduct(
                        supplier.Id,
                        productItem.ProductId,
                        productItem.UnitPrice.Value,
                        productItem.TierPrices,
                        productItem.UnitMeasureId ?? productUnitMeasureId,
                        now,
                        productItem.Currency ?? supplierDetailsCurrency));
            }

            return null;
        }

        private static bool HasDetailUpdate(SupplierDetailsInformation? details)
        {
            if (details is null)
            {
                return false;
            }

            return details.CreditDays is not null
                || details.HasCredit is not null
                || details.Address is not null
                || details.EmailSupport is not null
                || details.ContactName is not null
                || details.ContactEmail is not null
                || details.ContactPhoneNumber is not null
                || details.ExclusiveBrandsOrParts is not null
                || details.ExclusiveStatusComments is not null
                || details.SupplierType is not null
                || details.Currency is not null
                || details.CreditLimit is not null
                || details.CreditCurrency is not null
                || details.AlertDaysBeforeDue is not null
                || details.PreferredPaymentMethod is not null
                || details.ApplyIrRetention is not null
                || details.ApplyMunicipalRetention is not null
                || details.IsTaxExempt is not null;
        }
    }
}
