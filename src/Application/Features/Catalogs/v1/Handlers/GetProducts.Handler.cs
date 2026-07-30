using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class GetProductsHandler(IUnitOfWork unitOfWork, IErrorManager errorManager) : BaseValidatorHandler<GetProductsQuery, PagedResponse<ProductDto>>(unitOfWork, errorManager)
    {
        public override async Task<PagedResponse<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(
                request.UserId,
                request.CompanyId,
                request.ModuleCode,
                cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }
            var query = _unitOfWork.Products.Entities
                .AsNoTracking()
                .Where(c => !c.DeletedAt.HasValue);


            if (request.CategoryProductId.HasValue)
                query = query.Where(c => c.CategoryId == request.CategoryProductId.Value);


            var totalCount = await query.CountAsync(cancellationToken);

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new ProductDto
                {
                    ProductId = p.Id,
                    ProductName = p.ProductName!,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    Category = new ProductCategoryDto
                    {
                        Name = p.Category.Name!,
                        Code = p.Category.Code,
                        IsActive = p.Category.IsActive,
                    }
                })
                .ToListAsync(cancellationToken);

            return new PagedResponse<ProductDto>(
                products,
                request.PageNumber,
                request.PageSize,
                totalCount
            );
        }
    }
}

