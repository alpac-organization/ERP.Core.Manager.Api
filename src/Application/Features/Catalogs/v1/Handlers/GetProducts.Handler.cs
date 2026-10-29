using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class GetProductsHandler(IUnitOfWork unitOfWork, IErrorManager errorManager, IMapper _mapper) : BaseValidatorHandler<GetProductsQuery, PagedResponse<ProductDto>>(unitOfWork, errorManager)
    {
        public override async Task<PagedResponse<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(
                request.UserId,
                request.CompanyId,
                request.ModuleCode,
                cancellationToken
            );

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            var query = _unitOfWork.Products.Entities
                .AsNoTracking()
                .AsSplitQuery()

                .Include(product => product.Branch)
                .Include(product => product.Category)
                .Include(product => product.CostCenter)
                .Include(product => product.UnitMeasure)
                .Include(product => product.SupplierProducts)

                .Where(product => !product.DeletedAt.HasValue)
                .Where(product => product.BranchId == access.Profile.BranchId)
                .Where(product => product.ProductUsageType == request.ProductUsageType);

            if (access.Role?.RoleType == RoleType.Administrator && request.BranchId.HasValue)
            {
                query = query
                    .Where(product => product.BranchId == request.BranchId);
            }

            if (request.ProductUsageType == ProductUsageType.Operational)
            {
                //Logica de productos operacionales manejados desde inventario.
                query = query
                    .Where(product => product.CostCenterId == access.Profile.CostCenterId)
                    .Where(product => product.CostCenter.WorkAreaId == access.Profile.AreaId);
            }

            if (request.CategoryProductId.HasValue)
            {
                query = query.Where(c => c.CategoryId == request.CategoryProductId.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var productsMapped = _mapper.Map<List<ProductDto>>(products);

            return new PagedResponse<ProductDto>(
                productsMapped,
                request.PageNumber,
                request.PageSize,
                totalCount
            );
        }
    }
}

