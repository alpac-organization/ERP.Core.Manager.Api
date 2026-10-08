using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class UpdateSupplierInformationHandler(IUnitOfWork _unitOfWork, ILogger<UpdateSupplierInformationHandler> _logger, IErrorManager _errorManager) : BaseValidatorHandler<UpdateSupplierInformationCommand, bool>(_unitOfWork, _errorManager)
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
                _logger.LogWarning("Usuario {UserId} (Supervisor) intentó actualizar proveedor {SupplierId} sin permiso", request.UserId, request.SupplierId);
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para registrar un proveedor", "ERP:01");
            }

            var user = await _unitOfWork.Users.Entities
                .Where(user => user.Id == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Usuario desconocido!", "ERP:01");
            }

            var supplier = await _unitOfWork.Suppliers.Entities
                .Include(sup => sup.SupplierProducts)
                .Where(sup => sup.Id == request.SupplierId)
                .FirstOrDefaultAsync(cancellationToken);

            if (supplier is null)
            {
                return _errorManager.ThrowBadRequest<bool>("El registro de este proveedor no existe", "ERP:04");
            }

            if (access.Role!.RoleType != RoleType.Administrator && access.Role.RoleType != RoleType.Manager)
            {
                _logger.LogWarning("Usuario {UserId} con rol {RoleType} intentó actualizar proveedor {SupplierId} sin permiso", request.UserId, access.Role.RoleType, request.SupplierId);
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para actualizar este proveedor", "ERP:01");
            }

            supplier.SuppliersLegalName   = request.SuppliersLegalName   ?? supplier.SuppliersLegalName;
            supplier.CommercialName       = request.CommercialName       ?? supplier.CommercialName;
            supplier.IdentificationNumber = request.IdentificationNumber ?? supplier.IdentificationNumber;
            supplier.ConstitutionType     = request.ConstitutionType     ?? supplier.ConstitutionType;
            supplier.IdentificationType   = request.IdentificationType   ?? supplier.IdentificationType;

            if (request.SupplierDetails is not null)
            {
                var supplierDetails = await _unitOfWork.SuppliersDetails.Entities
                    .Where(supl => supl.SupplierId == supplier.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (supplierDetails is not null)
                {
                    supplierDetails.HasCredit                  = request.SupplierDetails.HasCredit                  ?? supplierDetails.HasCredit;
                    supplierDetails.CreditDays                 = request.SupplierDetails.CreditDays                 ?? supplierDetails.CreditDays;
                    supplierDetails.Address                    = request.SupplierDetails.Address                    ?? supplierDetails.Address;
                    supplierDetails.EmailSupport               = request.SupplierDetails.EmailSupport               ?? supplierDetails.EmailSupport;
                    supplierDetails.ContactName                = request.SupplierDetails.ContactName                ?? supplierDetails.ContactName;
                    supplierDetails.ContactEmail               = request.SupplierDetails.ContactEmail               ?? supplierDetails.ContactEmail;
                    supplierDetails.ContactPhoneNumber         = request.SupplierDetails.ContactPhoneNumber         ?? supplierDetails.ContactPhoneNumber;

                    supplierDetails.ExclusiveBrandsOrParts     = request.SupplierDetails.ExclusiveBrandsOrParts     ?? supplierDetails.ExclusiveBrandsOrParts;
                    supplierDetails.ExclusiveStatusComments    = request.SupplierDetails.ExclusiveStatusComments    ?? supplierDetails.ExclusiveStatusComments;
                    supplierDetails.SupplierType               = request.SupplierDetails.SupplierType               ?? supplierDetails.SupplierType;
                    supplierDetails.Currency                   = request.SupplierDetails.Currency                   ?? supplierDetails.Currency;
                    supplierDetails.CreditLimit                = request.SupplierDetails.CreditLimit                ?? supplierDetails.CreditLimit;
                    supplierDetails.CreditCurrency             = request.SupplierDetails.CreditCurrency             ?? supplierDetails.CreditCurrency;
                    supplierDetails.AlertDaysBeforeDue         = request.SupplierDetails.AlertDaysBeforeDue         ?? supplierDetails.AlertDaysBeforeDue;
                    supplierDetails.PreferredPaymentMethod     = request.SupplierDetails.PreferredPaymentMethod     ?? supplierDetails.PreferredPaymentMethod;
                    supplierDetails.ApplyIrRetention           = request.SupplierDetails.ApplyIrRetention           ?? supplierDetails.ApplyIrRetention;
                    supplierDetails.ApplyMunicipalRetention    = request.SupplierDetails.ApplyMunicipalRetention    ?? supplierDetails.ApplyMunicipalRetention;
                    supplierDetails.IsTaxExempt                = request.SupplierDetails.IsTaxExempt                ?? supplierDetails.IsTaxExempt;
                }
            }

            if (request.Products is { Count: > 0 })
            {
                var alreadyLinked = supplier.SupplierProducts
                    .Where(sp => sp.IsActive && sp.DeletedAt == null)
                    .Select(sp => sp.ProductId)
                    .ToHashSet();

                var (linkError, productUnitMeasures) = await SupplierProductLinkValidator.ValidateProductsToLinkAsync(
                    _unitOfWork,
                    request.Products,
                    alreadyLinked,
                    "ERP:ERROR_UPDATE",
                    cancellationToken);

                if (linkError is not null)
                {
                    return _errorManager.ThrowBadRequest<bool>(linkError.Message, linkError.Code);
                }

                var now = DateTime.UtcNow;

                foreach (var productItem in request.Products)
                {
                    supplier.SupplierProducts.Add(
                        SupplierProductPriceHelper.BuildSupplierProduct(
                            supplier.Id,
                            productItem.ProductId,
                            productItem.UnitPrice,
                            productItem.TierPrices,
                            productUnitMeasures[productItem.ProductId],
                            now));
                }
            }

            await _unitOfWork.Suppliers.UpdateAsync(supplier);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Usuario {UserId} actualizó el proveedor {SupplierId}", request.UserId, request.SupplierId);

            return true;
        }
    }
}
