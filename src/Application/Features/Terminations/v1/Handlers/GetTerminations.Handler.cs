using AutoMapper;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Queries;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Application.Features.Terminations.v1.Handlers
{
   public class GetTerminationsHandler(
      IUnitOfWork _unitOfWork,
      IErrorManager _errorManager, IMapper _mapper)
      : BaseValidatorHandler<GetTerminationsQuery, PagedResponse<TerminationDto>>(_unitOfWork, _errorManager)
   {
      public override async Task<PagedResponse<TerminationDto>> Handle(
         GetTerminationsQuery request,
         CancellationToken cancellationToken)
      {
         var access = await ValidateAccessAsync(
            request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

         if (!access.IsSuccess) return access.ErrorResponse!;

         var terminationQuery = _unitOfWork.CollaboratorTerminations.Entities
            .AsNoTracking()
            .Include(e => e.Collaborator)
            .Where(e =>
               e.DeletedAt == null &&
               e.Collaborator.CompanyId == request.CompanyId);

         if (!string.IsNullOrWhiteSpace(request.IdentificationNumber))
         {
            terminationQuery = terminationQuery
               .Where(e => e.Collaborator.IdentificationNumber == request.IdentificationNumber);
         }

         if (request.TerminationType.HasValue)
         {
            terminationQuery = terminationQuery
               .Where(e => e.TerminationType == request.TerminationType.Value);
         }

         if (request.TerminationStatus.HasValue)
         {
            terminationQuery = terminationQuery
               .Where(e => e.TerminationStatus == request.TerminationStatus.Value);
         }

         var totalCount = await terminationQuery.CountAsync(cancellationToken);

         List<CollaboratorTermination> terminations = await terminationQuery
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

         var mappedTerminations = _mapper.Map<List<TerminationDto>>(terminations);

         return new PagedResponse<TerminationDto>(
            mappedTerminations,
            request.PageNumber,
            request.PageSize,
            totalCount);
      }
   }
}
