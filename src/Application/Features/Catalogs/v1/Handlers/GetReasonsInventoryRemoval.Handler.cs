using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers;

public class ReasonsInventoryRemovalHandler(IUnitOfWork unitOfWork, IErrorManager errorManager, IMapper mapper) :
    BaseValidatorHandler<GetReasonsInventoryRemovalQuery, PagedResponse<ReasonsInventoryRemovalDto>>(unitOfWork, errorManager)
{
    private readonly IMapper _mapper = mapper;

    public override async Task<PagedResponse<ReasonsInventoryRemovalDto>> Handle(
        GetReasonsInventoryRemovalQuery request, CancellationToken ct)
    {
        var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode, ct);
        if (!access.IsSuccess) return access.ErrorResponse!;

        var query = _unitOfWork.ReasonsInventoryRemoval.Entities
            .AsNoTracking()
            .Where(r => !r.DeletedAt.HasValue)
            .Where(r => r.IsActive);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(r => r.Description)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var mapped = _mapper.Map<List<ReasonsInventoryRemovalDto>>(items);

        return new PagedResponse<ReasonsInventoryRemovalDto>(
            mapped,
            request.PageNumber,
            request.PageSize,
            totalCount
        );
    }
}