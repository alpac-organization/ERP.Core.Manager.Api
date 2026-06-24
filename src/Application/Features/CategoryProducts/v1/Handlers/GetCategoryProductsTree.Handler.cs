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

        // 2. Filtramos directamente en la base de datos (IQueryable) antes del ToListAsync.
        // Si request.ParentId es null, traerá solo los nodos raíz.
        // Si contiene un Guid, traerá únicamente los hijos directos de ese nodo.\
        var Categories = await _unitOfWork.CategoryProducts.Entities
            .Where(c => c.IsActive && c.ParentId == request.ParentId)
            .ToListAsync(cancellationToken);

        // 3. Mapea el listado plano obtenido
        return _mapper.Map<List<CategoryProductDto>>(Categories);
    }
}