using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers;

public class GetSupplierProductPriceHistoryHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager)
    : BaseValidatorHandler<GetSupplierProductPriceHistoryQuery, PagedResponse<SupplierProductPriceHistoryDto>>(_unitOfWork, _errorManager)
{
    public override async Task<PagedResponse<SupplierProductPriceHistoryDto>> Handle(
        GetSupplierProductPriceHistoryQuery request,
        CancellationToken cancellationToken)
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

        var supplierProductExists = await _unitOfWork.Products.Entities
            .AsNoTracking()
            .Where(p => p.Id == request.ProductId && p.DeletedAt == null)
            .SelectMany(p => p.SupplierProducts)
            .AnyAsync(sp =>
                sp.SupplierId == request.SupplierId &&
                sp.DeletedAt == null,
                cancellationToken);

        if (!supplierProductExists)
        {
            return _errorManager.ThrowBadRequest<PagedResponse<SupplierProductPriceHistoryDto>>(
                "La relación proveedor-producto no existe.",
                "ERP:PRICE_01");
        }

        var historyQuery = _unitOfWork.Products.Entities
            .AsNoTracking()
            .Where(p => p.Id == request.ProductId && p.DeletedAt == null)
            .SelectMany(p => p.SupplierProducts)
            .Where(sp => sp.SupplierId == request.SupplierId && sp.DeletedAt == null)
            .SelectMany(sp => sp.PriceHistories)
            .Where(h => h.DeletedAt == null);

        if (request.PriceType.HasValue)
        {
            historyQuery = historyQuery.Where(h => h.PriceType == request.PriceType.Value);
        }

        var totalCount = await historyQuery.CountAsync(cancellationToken);

        var history = await historyQuery
            .OrderByDescending(h => h.EffectiveFrom)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(h => new SupplierProductPriceHistoryDto
            {
                HistoryPriceId = h.Id,
                PriceType = h.PriceType,
                Price = h.Price,
                MinQuantity = h.MinQuantity,
                EffectiveFrom = h.EffectiveFrom,
                EffectiveTo = h.EffectiveTo
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<SupplierProductPriceHistoryDto>(
            history,
            request.PageNumber,
            request.PageSize,
            totalCount);
    }
}
