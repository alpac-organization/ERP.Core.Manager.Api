using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Services;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Commons.Mappings;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class RegisterProductHandler(IUnitOfWork _unitOfWork, ILogger<RegisterProductHandler> _logger, IErrorManager _errorManager, ICodeGenerator _codeGenerator) : BaseValidatorHandler<RegisterProductCommand, Guid>(_unitOfWork, _errorManager)
    {
        public override async Task<Guid> Handle(RegisterProductCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(
                request.UserId,
                request.CompanyId,
                request.ModuleCode!,
                cancellationToken
            );

            if (!access.IsSuccess)
            {
                return access.ErrorResponse;
            }

            if (access.Role?.RoleType == RoleType.Supervisor)
            {
                return _errorManager.ThrowBadRequest<Guid>(
                    "No tienes permiso para registrar un producto.",
                    "ERP:01"
                );
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

            var configuration = await _unitOfWork.CategoryAllowedUnitMeasures.Entities
                .Where(configuration => configuration.IsActive)
                .Where(configuration => configuration.CategoryId == request.CategoryId)
                .Where(configuration => configuration.UnitMeasureId == request.UnitMeasureId)
                .FirstOrDefaultAsync(cancellationToken);

            if (configuration is null)
            {
                return _errorManager.ThrowBadRequest<Guid>("Esta unidad de medida no es permitida para la categoria de producto seleccionada", "ERP:CONFIGURATION_INVALID");
            }
            
            var supplierCurrencies = new Dictionary<Guid, Currency>();

            if (request.Suppliers.Count > 0)
            {
                var supplierIds = request.Suppliers.Select(s => s.SupplierId).Distinct().ToList();

                supplierCurrencies = await _unitOfWork.SuppliersDetails.Entities
                    .AsNoTracking()
                    .Where(d => supplierIds.Contains(d.SupplierId))
                    .ToDictionaryAsync(d => d.SupplierId, d => d.Currency, cancellationToken);
            }

            var productEntity = ProductMapper.ToProductEntity(request, supplierCurrencies);

            productEntity.BranchId = access.Profile.BranchId;
            productEntity.CostCenterId = access.Profile.CostCenterId;


            if (request.ProductUsageType == ProductUsageType.Administrative)
            {
                var (isSuccess, code) = await _codeGenerator.GenerateUniqueProductCode(access.Profile, request.CategoryId, cancellationToken);

                if (!isSuccess || string.IsNullOrWhiteSpace(code))
                {
                    return _errorManager.ThrowBadRequest<Guid>(
                        "No se pudo generar el código del producto.",
                        "ERP:PROD01"
                    );
                }

                productEntity.Code = code;
                await _unitOfWork.Products.InsertProduct(productEntity);
            }
            else
            {
                //Validar si el area de origen es permitida para registro producto operacional
                var configurationAreaPermited = await _unitOfWork.InventoryAreasPermissions.Entities
                    .Where(configuration => configuration.IsActive)
                    .Where(configuration => configuration.WorkAreaId == access.Profile.AreaId)
                    .FirstOrDefaultAsync(cancellationToken);
                    
                if (configurationAreaPermited is null)
                {
                    return _errorManager.ThrowBadRequest<Guid>(
                        "El area al que pertence no tiene permitido el registro de productos operacionales",
                        "ERP:01"
                    );
                }

                //Validar producto operacional por parte de las areas.
                var (IsSuccess, Code) = await _codeGenerator.GenerateUniqueOperationalProductCode(access.Profile, request.CategoryId, cancellationToken);
                
                if (!IsSuccess || string.IsNullOrWhiteSpace(Code))
                {
                    return _errorManager.ThrowBadRequest<Guid>(
                        "No se pudo generar el código del producto.",
                        "ERP:PROD01"
                    );
                }

                productEntity.Code = Code;
                await _unitOfWork.Products.InsertProduct(productEntity);

                //Verificar si la categoria, si es diferentes de servicios, u otras cosas que no sean un categoria de producto real.
                var configurationInventory = await _unitOfWork.InventoryAllowedCategories.Entities
                    .Where(configuration => configuration.IsActive)
                    .Where(configuration => configuration.CategoryId == request.CategoryId)
                    .FirstOrDefaultAsync(cancellationToken);

                //Entrada a stock de inventario...
                if (configurationInventory is not null)
                {
                    var inventoryStockEntity = ProductMapper.ToProductStockEntity(request, productEntity.Id);
                    inventoryStockEntity.CostCenterId = access.Profile.CostCenterId;

                    await _unitOfWork.ProductStocks.RegisterProductStock(inventoryStockEntity);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return productEntity.Id;
        }
    }
}
