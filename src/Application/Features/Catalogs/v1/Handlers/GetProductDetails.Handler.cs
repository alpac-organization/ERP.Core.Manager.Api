using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

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
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.DeletedAt == null, cancellationToken);

        if (product is null)
        {
            return _errorManager.ThrowBadRequest<ProductDetailDto>("El producto no existe.", "ERP:PROD_NOT_FOUND");
        }

        var suppliersQuery = _unitOfWork.Products.Entities
            .AsNoTracking()
            .Where(p => p.Id == request.ProductId)
            .SelectMany(p => p.SupplierProducts)
            .Where(sp => sp.IsActive && sp.DeletedAt == null);

        var totalCount = await suppliersQuery.CountAsync(cancellationToken);

        var suppliers = await suppliersQuery
            .OrderBy(sp => sp.Supplier.SuppliersLegalName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
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
                    .Where(t => t.DeletedAt == null)
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
            .ToListAsync(cancellationToken);

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
            Suppliers = new PagedResponse<ProductSupplierDto>(
                suppliers,
                request.PageNumber,
                request.PageSize,
                totalCount)
        };
    }
}
