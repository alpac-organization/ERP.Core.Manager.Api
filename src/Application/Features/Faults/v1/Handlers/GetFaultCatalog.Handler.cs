using AutoMapper;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Queries;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Handlers
{
   public class GetFaultCatalogHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper)
      : BaseValidatorHandler<GetFaultCatalogQuery, PagedResponse<FaultCatalogDto>>(_unitOfWork, _errorManager)
   {
      public override async Task<PagedResponse<FaultCatalogDto>> Handle(GetFaultCatalogQuery request, CancellationToken cancellationToken)
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

         var faultCatalogQuery = _unitOfWork.FaultCatalogs.Entities
            .AsNoTracking()
            .Where(fc => fc.IsActive && fc.DeletedAt == null && fc.CompanyId == request.CompanyId);

         if (!string.IsNullOrWhiteSpace(request.Code))
         {
            var code = request.Code.Trim();

            faultCatalogQuery = faultCatalogQuery.
               Where(fc => fc.Code != null && fc.Code.Contains(code));
         }

         if (request.FaultSeverity.HasValue)
         {
            faultCatalogQuery = faultCatalogQuery.
               Where(fc => fc.FaultSeverity == request.FaultSeverity.Value);
         }

         var totalCount = await faultCatalogQuery.CountAsync(cancellationToken);

         List<FaultCatalog> faultCatalogs;

         if (request.PageSize.HasValue)
         {
            faultCatalogs = await faultCatalogQuery
               .OrderByDescending(fc => fc.CreatedAt)
               .ThenByDescending(fc => fc.FaultSeverity)
               .ThenBy(fc => fc.Code)
               .Skip((request.PageNumber - 1) * request.PageSize.Value)
               .Take(request.PageSize.Value)
               .ToListAsync(cancellationToken);
         }
         else
         {
            faultCatalogs = await faultCatalogQuery.ToListAsync(cancellationToken);
         }

         var mappedFaultCatalogs = _mapper.Map<List<FaultCatalogDto>>(faultCatalogs);

         return new PagedResponse<FaultCatalogDto>(
            mappedFaultCatalogs,
            request.PageNumber,
            request.PageSize ?? totalCount,
            totalCount
         );
      }
   }
}