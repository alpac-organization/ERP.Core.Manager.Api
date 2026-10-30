using AutoMapper;
using AutoMapper.QueryableExtensions;

using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Entities.Warehouse;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers;

public class GetProductDetailsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper) : BaseValidatorHandler<GetProductDetailsQuery, ProductDetailsDto>(_unitOfWork, _errorManager)
{
    public override async Task<ProductDetailsDto> Handle(GetProductDetailsQuery request, CancellationToken cancellationToken)
    {
        var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

        if (!access.IsSuccess)
        {
            return access.ErrorResponse!;
        }

        var product = await LoadProductAsync(request, cancellationToken);

        if (product is null)
        {
            return _errorManager.ThrowBadRequest<ProductDetailsDto>("El producto no existe.", "ERP:PROD_NOT_FOUND");
        }

        //Crear un mapeo para detalles de producto
        var productDetailsMapped = _mapper.Map<ProductDetailsDto>(product);
        var suppliersMapped = await LoadSuppliersAsync(request, cancellationToken);

        productDetailsMapped.Suppliers = suppliersMapped;

        return productDetailsMapped;
    }

    private async Task<Product?> LoadProductAsync(GetProductDetailsQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Products.Entities
            .AsNoTracking()
            .Include(p => p.Branch)
            .Include(p => p.Category)
            .Include(p => p.CostCenter)
            .Include(p => p.UnitMeasure)
            .Include(p => p.SupplierProducts)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.DeletedAt == null, cancellationToken);
    }

    private async Task<PagedResponse<ProductSupplierDto>> LoadSuppliersAsync(GetProductDetailsQuery request, CancellationToken cancellationToken)
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

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var suppliers = await suppliersQuery
            .OrderBy(sp => sp.Supplier.SuppliersLegalName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<ProductSupplierDto>(_mapper.ConfigurationProvider, new { today })
            .ToListAsync(cancellationToken);

        return new PagedResponse<ProductSupplierDto>(
            suppliers,
            request.PageNumber,
            request.PageSize,
            totalCount
        );
    }
}
