using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.CategoryProducts.v1.Dtos;

using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers;

public class GetCategoryProductsTreeHandler(IUnitOfWork unitOfWork, IErrorManager errorManager, IMapper _mapper) :BaseValidatorHandler<GetCategoryProductsQuery, List<CategoryProductDto>>(unitOfWork, errorManager)
{
    public override async Task<List<CategoryProductDto>> Handle(GetCategoryProductsQuery request, CancellationToken cancellationToken)
    {
        // 1. Validaciones de acceso obligatorias de tu arquitectura base
        var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode, cancellationToken);

        if (!access.IsSuccess)
        {
            return access.ErrorResponse!;
        }

        var Categories = await _unitOfWork.CategoryProducts.Entities
            .Where(c => c.IsActive && c.ParentId == request.ParentId)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<CategoryProductDto>>(Categories);
    }
}