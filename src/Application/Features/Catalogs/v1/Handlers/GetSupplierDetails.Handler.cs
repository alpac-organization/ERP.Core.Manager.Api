using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class GetSupplierDetailsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper)
        : BaseValidatorHandler<GetSupplierDetailsQuery, SupplierInformationDto>(_unitOfWork, _errorManager)
    {
        public override async Task<SupplierInformationDto> Handle(GetSupplierDetailsQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            var supplier = await _unitOfWork.Suppliers.Entities
                .AsNoTracking()
                .Include(sup => sup.SupplierDetails)
                .Include(sup => sup.SupplierBankAccounts)
                .Include(sup => sup.SupplierPaymentMethods)
                .Include(sup => sup.User)
                .Where(sup => sup.IsActive && sup.DeletedAt == null)
                .Where(sup => sup.Id == request.SupplierId)
                .FirstOrDefaultAsync(cancellationToken);

            if (supplier is null)
            {
                return _errorManager.ThrowBadRequest<SupplierInformationDto>("No se encontro registro de este proveedor", "ERP:NOT_FOUND");
            }

            var result = _mapper.Map<SupplierInformationDto>(supplier);
            result.SupplierPaymentMethods = SupplierPaymentMethodHelper
                .ActivePaymentMethods(supplier.SupplierPaymentMethods)
                .Select(pm => new SupplierPaymentMethods
                {
                    Notes = pm.Notes,
                    IsActive = pm.IsActive,
                    PaymentMethodType = pm.PaymentMethodType
                })
                .ToList();

            var productsQuery = _unitOfWork.Suppliers.Entities
                .AsNoTracking()
                .Where(sup => sup.Id == request.SupplierId)
                .SelectMany(sup => sup.SupplierProducts)
                .Where(sp => sp.IsActive && sp.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                productsQuery = productsQuery.Where(sp => sp.Product.Code.Contains(request.Code));
            }

            if (request.UnitMeasureId.HasValue)
            {
                productsQuery = productsQuery.Where(sp => sp.UnitMeasureId == request.UnitMeasureId.Value);
            }

            var totalCount = await productsQuery.CountAsync(cancellationToken);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var utcNow = DateTime.UtcNow;

            var products = await productsQuery
                .OrderBy(sp => sp.Product.Code)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(sp => new
                {
                    sp.ProductId,
                    Code = sp.Product.Code,
                    ProductName = sp.Product.ProductName!,
                    sp.UnitMeasureId,
                    sp.Currency,
                    sp.UnitPrice,
                    sp.LastPriceUpdate,
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

            var mappedProducts = products.Select(sp => new SupplierLinkedProductDto
            {
                ProductId = sp.ProductId,
                Code = sp.Code,
                ProductName = sp.ProductName,
                UnitMeasureId = sp.UnitMeasureId,
                Currency = sp.Currency,
                UnitPrice = sp.UnitPrice,
                LastPriceUpdate = sp.LastPriceUpdate,
                TierPrices = sp.TierPrices
                    .Where(t => SupplierProductPriceHelper.IsTierActiveOrUpcoming(t.ValidFrom, t.ValidTo, today))
                    .OrderByDescending(t => SupplierProductPriceHelper.IsTierCurrentlyValid(t.ValidFrom, t.ValidTo, today))
                    .ThenBy(t => t.ValidFrom)
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
                                    utcNow)
                                || h.EffectiveFrom > utcNow);

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
                    .ToList()
            }).ToList();

            result.Products = new PagedResponse<SupplierLinkedProductDto>(
                mappedProducts,
                request.PageNumber,
                request.PageSize,
                totalCount);

            return result;
        }
    }
}
