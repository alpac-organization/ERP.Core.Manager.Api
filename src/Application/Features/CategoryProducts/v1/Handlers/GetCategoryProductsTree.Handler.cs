using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.CategoryProducts.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.CategoryProducts.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.CategoryProducts.v1.Handlers;

public class GetCategoryProductsTreeHandler(IUnitOfWork unitOfWork, IErrorManager errorManager, IMapper mapper)
    : AlpacBaseHandler<GetCategoryProductsTreeQuery, List<CategoryProductDto>>(unitOfWork, errorManager)
{
    private readonly IMapper _mapper = mapper;

    public override async Task<List<CategoryProductDto>> Handle(GetCategoryProductsTreeQuery request, CancellationToken cancellationToken)
    {
        // 1. Validaciones de acceso obligatorias de tu arquitectura base
        var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode, cancellationToken);
        if (!access.IsSuccess)
        {
            return access.ErrorResponse!;
        }

        // 2. Traemos todas las categorías activas a memoria en una sola consulta plana
        var allCategories = await _unitOfWork.CategoryProducts.Entities
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);

        // 3. CAMBIO: Filtramos las raíces usando la propiedad real del Core 'ParentId'
        var rootCategories = allCategories
            .Where(c => c.ParentId == null)
            .ToList();

        // 4. Mapeamos hacia el DTO (AutoMapper se encargará de traducir la estructura recursiva)
        return _mapper.Map<List<CategoryProductDto>>(rootCategories);
    }
}