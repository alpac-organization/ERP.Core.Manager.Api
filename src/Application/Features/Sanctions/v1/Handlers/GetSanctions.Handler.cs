using AutoMapper;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Queries;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Handlers
{
   public class GetSanctionsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper)
      : BaseValidatorHandler<GetSanctionsQuery, PagedResponse<SanctionDto>>(_unitOfWork, _errorManager)
   {
      public override async Task<PagedResponse<SanctionDto>> Handle(
         GetSanctionsQuery request,
         CancellationToken cancellationToken)
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

         var sanctionsQuery = _unitOfWork.DisciplinarySanctions.Entities
            .AsNoTracking()
            .Include(s => s.Fault)
               .ThenInclude(f => f.Collaborator)
            .Where(s =>
               s.DeletedAt == null
               && s.Fault.DeletedAt == null
               && s.Fault.Collaborator.CompanyId == request.CompanyId);

         if (request.CollaboratorId.HasValue)
         {
            sanctionsQuery = sanctionsQuery
               .Where(s => s.Fault.CollaboratorId == request.CollaboratorId.Value);
         }

         if (request.FaultId.HasValue)
         {
            sanctionsQuery = sanctionsQuery
               .Where(s => s.FaultId == request.FaultId.Value);
         }

         if (request.SanctionType.HasValue)
         {
            sanctionsQuery = sanctionsQuery
               .Where(s => s.SanctionType == request.SanctionType.Value);
         }

         if (request.SanctionStatus.HasValue)
         {
            sanctionsQuery = sanctionsQuery
               .Where(s => s.SanctionStatus == request.SanctionStatus.Value);
         }

         var totalCount = await sanctionsQuery.CountAsync(cancellationToken);

         List<DisciplinarySanction> sanctions = await sanctionsQuery
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

         var mappedSanctions = _mapper.Map<List<SanctionDto>>(sanctions);

         return new PagedResponse<SanctionDto>(
            mappedSanctions,
            request.PageNumber,
            request.PageSize,
            totalCount);
      }
   }
}
