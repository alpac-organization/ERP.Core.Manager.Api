using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers;

public class GetProductDetailsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager)
    : BaseValidatorHandler<GetProductDetailsQuery, ProductDetailDto>(_unitOfWork, _errorManager)
{
    public override async Task<ProductDetailDto> Handle(GetProductDetailsQuery request, CancellationToken cancellationToken)
    {
        var access = await ValidateAccessAsync(
            request.UserId,
            request.CompanyId,
            request.ModuleCode!,
            cancellationToken);

        if (!access.IsSuccess)
        {
            return access.ErrorResponse!;
        }

        var product = await _unitOfWork.Products.Entities
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.UnitMeasure)
            .Include(p => p.SupplierProducts.Where(sp => sp.IsActive && sp.DeletedAt == null))
            .ThenInclude(sp => sp.Supplier)
            .Include(p => p.SupplierProducts.Where(sp => sp.IsActive && sp.DeletedAt == null))
            .ThenInclude(sp => sp.TierPrices.Where(t => t.DeletedAt == null))
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.DeletedAt == null, cancellationToken);

        if (product is null)
        {
            return _errorManager.ThrowBadRequest<ProductDetailDto>("El producto no existe.", "ERP:PROD_NOT_FOUND");
        }

        return new ProductDetailDto
        {
            ProductId = product.Id,
            Code = product.Code,
            ProductName = product.ProductName!,
            Description = product.Description,
            CategoryId = product.CategoryId,
            Category = new ProductCategoryDto
            {
                Name = product.Category.Name!,
                Code = product.Category.Code,
                IsActive = product.Category.IsActive
            },
            UnitMeasureId = product.UnitMeasureId,
            UnitMeasureName = product.UnitMeasure?.Name,
            ProductUsageType = product.ProductUsageType,
            IsTaxExempt = product.IsTaxExempt,
            Suppliers = product.SupplierProducts
                .Select(sp => new ProductSupplierDto
                {
                    SupplierProductId = sp.Id,
                    SupplierId = sp.SupplierId,
                    SupplierLegalName = sp.Supplier.SuppliersLegalName,
                    CommercialName = sp.Supplier.CommercialName,
                    UnitPrice = sp.UnitPrice,
                    LastPriceUpdate = sp.LastPriceUpdate,
                    IsActive = sp.IsActive,
                    TierPrices = sp.TierPrices
                        .Select(t => new TierPriceResponseDto
                        {
                            TierPriceId = t.Id,
                            MinQuantity = t.MinQuantity,
                            PreferentialPrice = t.PreferentialPrice,
                            ValidFrom = t.ValidFrom,
                            ValidTo = t.ValidTo,
                            UnitMeasureId = t.UnitMeasureId
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}
