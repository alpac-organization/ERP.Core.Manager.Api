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
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            var roleType        = access.Role?.RoleType;
            var isManager       = roleType == RoleType.Manager;
            var isAdministrator = roleType == RoleType.Administrator;
            var hasFullAccess   = isAdministrator || isManager;
            var isOperational   = request.ProductUsageType == ProductUsageType.Operational;

            var query = _unitOfWork.Products.Entities
                .AsNoTracking()
                .AsSplitQuery()

                .Include(product => product.Branch)
                .Include(product => product.Category)
                .Include(product => product.CostCenter)
                .Include(product => product.UnitMeasure)
                .Include(product => product.SupplierProducts)

                .Where(product => product.ProductUsageType == request.ProductUsageType);

            if (isOperational)
            {
                // Sucursal
                if (isAdministrator)
                {
                    if (request.BranchId.HasValue)
                    {
                        query = query
                            .Where(product => product.BranchId == request.BranchId.Value);
                    }
                }
                else
                {
                    query = query
                        .Where(product => product.BranchId == access.Profile.BranchId);
                }

                // Centro de costo
                if (hasFullAccess)
                {
                    if (request.CostCenterId.HasValue)
                    {
                        query = query
                            .Where(product => product.CostCenterId == request.CostCenterId.Value);
                    }
                }
                else
                {
                    query = query
                        .Where(product => product.CostCenterId == access.Profile.CostCenterId)
                        .Where(product => product.CostCenter.WorkAreaId == access.Profile.AreaId);
                }
            }
            else
            {
                query = query
                    .Where(product => product.BranchId == access.Profile.BranchId);
            }

            if (request.CategoryProductId.HasValue)
            {
                query = query
                    .Where(product => product.CategoryId == request.CategoryProductId.Value);
            }

            if (request.OnlyActive)
            {
                query = query
                    .Where(product => product.ProductStatus == ProductStatus.Active);
            }

            if (!string.IsNullOrEmpty(request.ProductName))
            {
                query = query
                    .Where(product => product.ProductName!.Contains(request.ProductName));
            }

            if (!string.IsNullOrEmpty(request.ProductCode))
            {
                query = query
                    .Where(product => product.Code == request.ProductCode);
            }

            if (request.Status.HasValue)
            {
                query = query
                    .Where(product => product.ProductStatus == request.Status);
            }

            var totalCount = await query
                .CountAsync(cancellationToken);

            var products = await query
                .OrderByDescending(product => product.CreatedAt)
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