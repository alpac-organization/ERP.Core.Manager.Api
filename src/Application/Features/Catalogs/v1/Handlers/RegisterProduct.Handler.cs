using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Services;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Commons.Mappings;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

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

            var roleType        = access.Role?.RoleType;
            var isAdministrator = roleType == RoleType.Administrator;
            var isManager       = roleType == RoleType.Manager;
            var isOperational   = request.ProductUsageType == ProductUsageType.Operational;

            // Los supervisores no pueden registrar productos
            if (roleType == RoleType.Supervisor)
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

            // ---------- Sucursal y centro de costo efectivos ----------
            Guid branchId;
            Guid costCenterId;

            if (isOperational)
            {
                var canChooseBranch     = isAdministrator;                  // agrega "|| isManager" si el manager también elige sucursal
                var canChooseCostCenter = isAdministrator || isManager;

                // Administrador: elige sucursal. Manager y operador: la del perfil.
                branchId = canChooseBranch
                    ? request.BranchId ?? access.Profile.BranchId
                    : access.Profile.BranchId;

                // Administrador y manager: eligen centro de costo. Operador: el del perfil.
                costCenterId = canChooseCostCenter
                    ? request.CostCenterId ?? access.Profile.CostCenterId
                    : access.Profile.CostCenterId;

                if (canChooseCostCenter)
                {
                    var costCenter = await _unitOfWork.CostCenters.Entities
                        .AsNoTracking()
                        .Where(cc => cc.Id == costCenterId)
                        .Select(cc => new { cc.BranchId, cc.IsActive, cc.IsOperational })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (costCenter is null || !costCenter.IsActive)
                    {
                        return _errorManager.ThrowBadRequest<Guid>(
                            "El centro de costo seleccionado no existe o no está activo.",
                            "ERP:CC01"
                        );
                    }

                    if (!costCenter.IsOperational)
                    {
                        return _errorManager.ThrowBadRequest<Guid>(
                            "El centro de costo seleccionado no es operacional.",
                            "ERP:CC02"
                        );
                    }

                    if (costCenter.BranchId != branchId)
                    {
                        return _errorManager.ThrowBadRequest<Guid>(
                            "El centro de costo seleccionado no pertenece a la sucursal indicada.",
                            "ERP:CC03"
                        );
                    }
                }
            }
            else
            {
                // Productos administrativos: mismo comportamiento que antes
                branchId     = access.Profile.BranchId;
                costCenterId = request.CostCenterId ?? access.Profile.CostCenterId;
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

            productEntity.BranchId     = branchId;
            productEntity.CostCenterId = costCenterId;

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
                // Validar si el área de origen es permitida para registro de producto operacional
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

                var (IsSuccess, Code) = await _codeGenerator.GenerateUniqueOperationalProductCode(
                    access.Profile,
                    request.CategoryId,
                    costCenterId,
                    cancellationToken
                );

                if (!IsSuccess || string.IsNullOrWhiteSpace(Code))
                {
                    return _errorManager.ThrowBadRequest<Guid>(
                        "No se pudo generar el código del producto.",
                        "ERP:PROD01"
                    );
                }

                productEntity.Code = Code;
                await _unitOfWork.Products.InsertProduct(productEntity);

                // Verificar si la categoría es de producto real (no servicios u otros)
                var configurationInventory = await _unitOfWork.CategoryProducts.Entities
                    .Where(category => category.IsActive)
                    .Where(category => !category.IsService)
                    .Where(category => category.CategoryType == CategoryType.Operational)
                    .Where(category => category.Id == request.CategoryId)
                    .FirstOrDefaultAsync(cancellationToken);

                // Entrada a stock de inventario
                if (configurationInventory is not null)
                {
                    var inventoryStockEntity = ProductMapper.ToProductStockEntity(request, productEntity.Id);
                    inventoryStockEntity.CostCenterId = costCenterId;

                    await _unitOfWork.ProductStocks.RegisterProductStock(inventoryStockEntity);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return productEntity.Id;
        }
    }
}