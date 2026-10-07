using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers;

public class GetSupplierProductPriceHistoryHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager)
    : BaseValidatorHandler<GetSupplierProductPriceHistoryQuery, List<SupplierProductPriceHistoryDto>>(_unitOfWork, _errorManager)
{
    public override async Task<List<SupplierProductPriceHistoryDto>> Handle(
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

        var product = await _unitOfWork.Products.Entities
            .AsNoTracking()
            .Include(p => p.SupplierProducts.Where(sp =>
                sp.SupplierId == request.SupplierId &&
                sp.DeletedAt == null))
            .ThenInclude(sp => sp.PriceHistories)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.DeletedAt == null, cancellationToken);

        var supplierProduct = product?.SupplierProducts.FirstOrDefault();

        if (supplierProduct is null)
        {
            return _errorManager.ThrowBadRequest<List<SupplierProductPriceHistoryDto>>(
                "La relación proveedor-producto no existe.",
                "ERP:PRICE_01");
        }

        return supplierProduct.PriceHistories
            .Where(h => h.DeletedAt == null)
            .OrderByDescending(h => h.EffectiveFrom)
            .Select(h => new SupplierProductPriceHistoryDto
            {
                HistoryPriceId = h.Id,
                PriceType = h.PriceType,
                Price = h.Price,
                MinQuantity = h.MinQuantity,
                EffectiveFrom = h.EffectiveFrom,
                EffectiveTo = h.EffectiveTo
            })
            .ToList();
    }
}
