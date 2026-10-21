using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Warehouse;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Services;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class RegisterProductHandler(IUnitOfWork _unitOfWork, ILogger<RegisterProductHandler> _logger, IErrorManager _errorManager, ICodeGenerator _codeGenerator)
        : BaseValidatorHandler<RegisterProductCommand, Guid>(_unitOfWork, _errorManager)
    {
        public override async Task<Guid> Handle(RegisterProductCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(
                request.UserId,
                request.CompanyId,
                request.ModuleCode!,
                cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse;
            }

            if (access.Role?.RoleType == RoleType.Supervisor)
            {
                return _errorManager.ThrowBadRequest<Guid>(
                    "No tienes permiso para registrar un producto.",
                    "ERP:01");
            }

            var categoryExists = await _unitOfWork.CategoryProducts.Entities
                .AnyAsync(c => c.Id == request.CategoryId && c.IsActive && c.DeletedAt == null, cancellationToken);

            if (!categoryExists)
            {
                return _errorManager.ThrowBadRequest<Guid>(
                    "La categoría seleccionada no existe o no está activa.",
                    "ERP:002");
            }

            var unitMeasureExists = await _unitOfWork.UnitsMeasurement.Entities
                .AnyAsync(u => u.Id == request.UnitMeasureId && u.IsActive, cancellationToken);

            if (!unitMeasureExists)
            {
                return _errorManager.ThrowBadRequest<Guid>(
                    "La unidad de medida seleccionada no existe.",
                    "ERP:PROD02");
            }

            var linkError = await SupplierProductLinkValidator.ValidateSuppliersToLinkAsync(
                _unitOfWork,
                request.Suppliers,
                alreadyLinkedSupplierIds: null,
                cancellationToken);

            if (linkError is not null)
            {
                return _errorManager.ThrowBadRequest<Guid>(linkError.Message, linkError.Code);
            }

            _logger.LogInformation("Iniciando registro de producto: {ProductName}", request.ProductName);

            var supplierCurrencies = new Dictionary<Guid, Currency>();

            if (request.Suppliers.Count > 0)
            {
                var supplierIds = request.Suppliers.Select(s => s.SupplierId).Distinct().ToList();

                supplierCurrencies = await _unitOfWork.SuppliersDetails.Entities
                    .AsNoTracking()
                    .Where(d => supplierIds.Contains(d.SupplierId))
                    .ToDictionaryAsync(d => d.SupplierId, d => d.Currency, cancellationToken);
            }

            var now = DateTime.UtcNow;
            Guid productId = Guid.Empty;

            await _unitOfWork.ExecuteWithUniqueCodeRetryAsync(async token =>
            {
                var (isSuccess, code) = await _codeGenerator.GenerateUniqueProductCode(
                    access.Profile.BranchId,
                    request.CategoryId,
                    token);

                if (!isSuccess || string.IsNullOrWhiteSpace(code))
                {
                    _errorManager.ThrowBadRequest<Guid>(
                        "No se pudo generar el código del producto.",
                        "ERP:PROD01");
                    return;
                }

                var productEntity = new Product
                {
                    Code = code,
                    BranchId = access.Profile.BranchId,
                    CostCenterId = request.ProductUsageType == ProductUsageType.Operational ? access.Profile.CostCenterId : null,
                    ProductName = request.ProductName,
                    CategoryId = request.CategoryId,
                    Description = request.Description,
                    IsTaxExempt = request.IsTaxExempt,
                    UnitMeasureId = request.UnitMeasureId,
                    ProductUsageType = request.ProductUsageType,

                    SupplierProducts = [.. request.Suppliers
                        .Select(s => SupplierProductPriceHelper.BuildSupplierProduct(
                            s.SupplierId,
                            productId: null,
                            s.UnitPrice,
                            s.TierPrices,
                            s.UnitMeasureId ?? request.UnitMeasureId,
                            now,
                            s.Currency ?? supplierCurrencies.GetValueOrDefault(s.SupplierId, Currency.NIO)))
                        ]
                };

                await _unitOfWork.Products.InsertProduct(productEntity);

                //Registrar producto en stock
                if (request.ProductUsageType == ProductUsageType.Operational)
                {
                    
                }

                await _unitOfWork.SaveChangesAsync(token);

                productId = productEntity.Id;

                _logger.LogInformation(
                    "Producto {ProductId} registrado exitosamente con código {Code}",
                    productEntity.Id,
                    productEntity.Code
                );

            }, cancellationToken);

            return productId;
        }
    }
}
