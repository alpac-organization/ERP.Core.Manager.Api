using AutoMapper;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Queries;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Handlers
{

   public class GetPermitTypePoliciesHandler(IUnitOfWork unitOfWork, IErrorManager errorManager, IMapper mapper)
      : BaseValidatorHandler<GetPermitTypePoliciesQuery, PagedResponse<PermitTypePolicyDto>>(unitOfWork, errorManager)
   {
     public override async Task<PagedResponse<PermitTypePolicyDto>> Handle(GetPermitTypePoliciesQuery request, CancellationToken cancellationToken)
      {
         var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

         if (!access.IsSuccess)
         {
            return access.ErrorResponse!;
         }

         var queryPermitTypePolicies = _unitOfWork.PermitTypePolicyRepository.Entities
            .AsNoTracking()
            .Where(policy => policy.CompanyId == request.CompanyId);


         if (request.OnlyActive)
         {
            queryPermitTypePolicies = queryPermitTypePolicies.Where(policy=> policy.IsActive);
         }

         if (request.PermitApplicationType.HasValue)
         {
            queryPermitTypePolicies = queryPermitTypePolicies.Where(policy => policy.PermitApplicationType == request.PermitApplicationType);
         }

         var total = await queryPermitTypePolicies.CountAsync(cancellationToken);

         var policies = await queryPermitTypePolicies
             .OrderBy(policy=> policy.PermitApplicationType)
             .ThenBy(p=>p.FamilyRelationship)
             .Skip((request.PageNumber - 1) * request.PageSize)
             .Take(request.PageSize)
             .ToListAsync(cancellationToken);

         return new PagedResponse<PermitTypePolicyDto>(
            mapper.Map<List<PermitTypePolicyDto>>(policies),
            request.PageNumber,
            request.PageSize,
            total);
      } 
   }
}