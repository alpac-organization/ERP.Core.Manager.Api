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
   public class GetFaultsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper)
      : BaseValidatorHandler<GetFaultsQuery, PagedResponse<FaultDto>>(_unitOfWork, _errorManager)
   {
      public override async Task<PagedResponse<FaultDto>> Handle(GetFaultsQuery request, CancellationToken cancellationToken)
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

         var faultsQuery = _unitOfWork.DisciplinaryFaults.Entities
            .AsNoTracking()
            .Include(f => f.Collaborator)
            .Include(f => f.FaultCatalog)
            .Where(f => f.DeletedAt == null && f.Collaborator.CompanyId == request.CompanyId);

         if (!string.IsNullOrWhiteSpace(request.Code))
         {
            var code = request.Code.Trim();

            faultsQuery = faultsQuery
               .Where(f => f.FaultCatalog.Code != null && f.FaultCatalog.Code.Contains(code));
         }

         if (request.Severity.HasValue)
         {
            faultsQuery = faultsQuery
               .Where(f => f.Severity == request.Severity.Value);
         }

         if (request.Status.HasValue)
         {
            faultsQuery = faultsQuery
               .Where(f => f.Status == request.Status.Value);
         }

         if (request.Source.HasValue)
         {
            faultsQuery = faultsQuery
               .Where(f => f.Source == request.Source.Value);
         }

         var totalCount = await faultsQuery.CountAsync(cancellationToken);

         List<DisciplinaryFault> faults = await faultsQuery
            .OrderByDescending(f => f.CreatedAt)
            .ThenByDescending(f => f.Severity)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

         var mappedFaults = _mapper.Map<List<FaultDto>>(faults);

         return new PagedResponse<FaultDto>(
            mappedFaults,
            request.PageNumber,
            request.PageSize,
            totalCount
         );
      }
   }
}
