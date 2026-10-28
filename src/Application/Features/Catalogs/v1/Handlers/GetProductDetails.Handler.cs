using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Warehouse;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers;

public class GetProductDetailsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager) : BaseValidatorHandler<GetProductDetailsQuery, ProductDetailDto>(_unitOfWork, _errorManager)
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

        var product = await LoadProductAsync(request.ProductId, cancellationToken);

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
            Category = new CategoryProductDto
            {
                Name = product.Category.Name!,
                Code = product.Category.Code,
                IsActive = product.Category.IsActive
            },
            UnitMeasureId = product.UnitMeasureId,
            UnitMeasureName = product.UnitMeasure?.Name,
            ProductUsageType = product.ProductUsageType,
            IsTaxExempt = product.IsTaxExempt,
            Suppliers = await LoadSuppliersAsync(request, cancellationToken)
        };
    }

    private async Task<Product?> LoadProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Products.Entities
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.UnitMeasure)
            .FirstOrDefaultAsync(p => p.Id == productId && p.DeletedAt == null, cancellationToken);
    }

    private async Task<PagedResponse<ProductSupplierDto>> LoadSuppliersAsync(
        GetProductDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var suppliersQuery = _unitOfWork.Products.Entities
            .AsNoTracking()
            .Where(p => p.Id == request.ProductId)
            .SelectMany(p => p.SupplierProducts)
            .Where(sp => sp.IsActive && sp.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(request.CommercialName))
        {
            suppliersQuery = suppliersQuery
                .Where(sp =>
                    sp.Supplier.CommercialName != null &&
                    sp.Supplier.CommercialName.Contains(request.CommercialName));
        }

        if (!string.IsNullOrWhiteSpace(request.IdentificationNumber))
        {
            suppliersQuery = suppliersQuery
                .Where(sp => sp.Supplier.IdentificationNumber == request.IdentificationNumber);
        }

        if (request.ExclusiveStatus.HasValue)
        {
            suppliersQuery = suppliersQuery
                .Where(sp =>
                    sp.Supplier.SupplierDetails != null &&
                    sp.Supplier.SupplierDetails.ExclusiveStatus == request.ExclusiveStatus.Value);
        }

        var totalCount = await suppliersQuery.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return new PagedResponse<ProductSupplierDto>([], request.PageNumber, request.PageSize, 0);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var utcNow = DateTime.UtcNow;

        var suppliers = await suppliersQuery
            .OrderBy(sp => sp.Supplier.SuppliersLegalName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(sp => new
            {
                SupplierProductId = sp.Id,
                sp.SupplierId,
                SupplierLegalName = sp.Supplier.SuppliersLegalName,
                CommercialName = sp.Supplier.CommercialName,
                SupplierType = sp.Supplier.SupplierDetails != null
                    ? sp.Supplier.SupplierDetails.SupplierType
                    : SupplierType.Ordinary,
                sp.Currency,
                sp.UnitMeasureId,
                sp.UnitPrice,
                sp.LastPriceUpdate,
                sp.IsActive,
                TierPrices = sp.TierPrices
                    .Where(t => t.DeletedAt == null)
                    .Select(t => new
                    {
                        t.Id,
                        t.MinQuantity,
                        t.PreferentialPrice,
                        t.ValidFrom,
                        t.ValidTo,
                        t.UnitMeasureId
                    })
                    .ToList(),
                PreferentialHistories = sp.PriceHistories
                    .Where(h =>
                        h.DeletedAt == null &&
                        h.PriceType == SupplierPriceHistoryType.PreferentialPrice)
                    .Select(h => new
                    {
                        h.MinQuantity,
                        h.Currency,
                        h.EffectiveFrom,
                        h.EffectiveTo
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var mappedSuppliers = suppliers.Select(sp =>
        {
            var tiers = sp.TierPrices
                .Where(t => SupplierProductPriceHelper.IsTierCurrentlyValid(t.ValidFrom, t.ValidTo, today))
                .OrderBy(t => t.ValidFrom)
                .Take(1)
                .Select(t =>
                {
                    var matchingHistory = sp.PreferentialHistories
                        .Where(h => h.MinQuantity == t.MinQuantity)
                        .OrderByDescending(h => h.EffectiveFrom)
                        .FirstOrDefault(h =>
                            SupplierProductPriceHelper.IsCurrentPrice(
                                h.EffectiveFrom,
                                SupplierProductPriceHelper.ToApiEffectiveTo(h.EffectiveTo),
                                utcNow));

                    return new TierPriceResponseDto
                    {
                        TierPriceId = t.Id,
                        MinQuantity = t.MinQuantity,
                        PreferentialPrice = t.PreferentialPrice,
                        ValidFrom = t.ValidFrom,
                        ValidTo = t.ValidTo,
                        UnitMeasureId = t.UnitMeasureId,
                        Currency = matchingHistory?.Currency ?? sp.Currency
                    };
                })
                .ToList();

            return new ProductSupplierDto
            {
                SupplierProductId = sp.SupplierProductId,
                SupplierId = sp.SupplierId,
                SupplierLegalName = sp.SupplierLegalName,
                CommercialName = sp.CommercialName,
                SupplierType = sp.SupplierType,
                Currency = sp.Currency,
                UnitMeasureId = sp.UnitMeasureId,
                UnitPrice = sp.UnitPrice,
                LastPriceUpdate = sp.LastPriceUpdate,
                IsActive = sp.IsActive,
                TierPrices = tiers
            };
        }).ToList();

        return new PagedResponse<ProductSupplierDto>(
            mappedSuppliers,
            request.PageNumber,
            request.PageSize,
            totalCount);
    }
}
