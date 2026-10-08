using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Domain.Entities.Warehouse;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Application.Commons.Interfaces.Services;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers;

public class RegisterProductHandler(
    IUnitOfWork _unitOfWork,
    ILogger<RegisterProductHandler> _logger,
    IErrorManager _errorManager,
    ICodeGenerator _codeGenerator)
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

        var (isSuccess, code) = await _codeGenerator.GenerateUniqueProductCode(
            request.CompanyId,
            request.CategoryId,
            cancellationToken);

        if (!isSuccess || string.IsNullOrWhiteSpace(code))
        {
            return _errorManager.ThrowBadRequest<Guid>(
                "No se pudo generar el código del producto.",
                "ERP:PROD01");
        }

        _logger.LogInformation("Iniciando registro de producto: {ProductName}", request.ProductName);

        var now = DateTime.UtcNow;

        var productEntity = new Product
        {
            Code = code,
            ProductName = request.ProductName,
            Description = request.Description,
            CategoryId = request.CategoryId,
            UnitMeasureId = request.UnitMeasureId,
            ProductUsageType = request.ProductUsageType,
            IsTaxExempt = request.IsTaxExempt,
            SupplierProducts = request.Suppliers
                .Select(s => SupplierProductPriceHelper.BuildSupplierProduct(
                    s.SupplierId,
                    productId: null,
                    s.UnitPrice,
                    s.TierPrices,
                    s.UnitMeasureId ?? request.UnitMeasureId,
                    now))
                .ToList()
        };

        await _unitOfWork.Products.InsertProduct(productEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Producto {ProductId} registrado exitosamente con código {Code}", productEntity.Id, productEntity.Code);

        return productEntity.Id;
    }
}
