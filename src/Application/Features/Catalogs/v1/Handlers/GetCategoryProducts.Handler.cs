using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers;

public class GetCategoryProductsTreeHandler(
    IUnitOfWork _unitOfWork,
    IErrorManager _errorManager,
    IMapper mapper)
    : BaseValidatorHandler<GetCategoryProductsQuery, List<CategoryProductDto>>(_unitOfWork, _errorManager)
{
    public override async Task<List<CategoryProductDto>> Handle(GetCategoryProductsQuery request, CancellationToken cancellationToken)
    {
        var access = await ValidateAccessAsync(
            request.UserId,
            request.CompanyId,
            request.ModuleCode,
            cancellationToken);

        if (!access.IsSuccess)
            return access.ErrorResponse!;

        // Obtener TODAS las categorías activas (sin importar el padre)
        var allCategories = await _unitOfWork.CategoryProducts.Entities
            .AsNoTracking()
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);

        // Mapear a DTOs
        var categoryDtos = mapper.Map<List<CategoryProductDto>>(allCategories);

        // Construir el árbol usando un diccionario
        var dict = categoryDtos.ToDictionary(c => c.Id);
        var rootCategories = new List<CategoryProductDto>();

        foreach (var dto in categoryDtos)
        {
            if (dto.CategoryId.HasValue && dict.ContainsKey(dto.CategoryId.Value))
            {
                // Es hijo de otro → agregar a la lista de hijos del padre
                dict[dto.CategoryId.Value].SubCategory.Add(dto);
            }
            else
            {
                // Es raíz (sin padre o padre inactivo)
                rootCategories.Add(dto);
            }
        }

        return rootCategories;
    }
}