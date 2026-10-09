using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class GetSupplierDetailsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper) : BaseValidatorHandler<GetSupplierDetailsQuery, SupplierInformationDto>(_unitOfWork, _errorManager)
    {
        public override async Task<SupplierInformationDto> Handle(GetSupplierDetailsQuery request, CancellationToken cancellationToken)
        {
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
                productsQuery = productsQuery.Where(sp => sp.Product.UnitMeasureId == request.UnitMeasureId.Value);
            }

            var totalCount = await productsQuery.CountAsync(cancellationToken);

            var products = await productsQuery
                .OrderBy(sp => sp.Product.Code)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(sp => new SupplierLinkedProductDto
                {
                    ProductId = sp.ProductId,
                    Code = sp.Product.Code,
                    ProductName = sp.Product.ProductName!,
                    UnitMeasureId = sp.Product.UnitMeasureId,
                    Currency = sp.Currency,
                    UnitPrice = sp.UnitPrice,
                    LastPriceUpdate = sp.LastPriceUpdate,
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

            result.Products = new PagedResponse<SupplierLinkedProductDto>(
                products,
                request.PageNumber,
                request.PageSize,
                totalCount);

            return result;
        }
    }
}
