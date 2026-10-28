using AutoMapper;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Queries;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Handlers
{

   public class GetPermitTypePoliciesHandler(IUnitOfWork unitOfWork, IErrorManager errorManager, IMapper mapper)
      : BaseValidatorHandler<GetPermitTypePoliciesQuery, List<PermitTypePolicyDto>>(unitOfWork, errorManager)
   {
     public override async Task<List<PermitTypePolicyDto>> Handle(GetPermitTypePoliciesQuery request, CancellationToken cancellationToken)
      {
         var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

         if (!access.IsSuccess)
         {
            return access.ErrorResponse!;
         }

         var queryPermitTypePolicies = _unitOfWork.PermitTypePolicyRepository.Entities.AsNoTracking();


         if (request.OnlyActive)
         {
            queryPermitTypePolicies = queryPermitTypePolicies.Where(policy=> policy.IsActive);
         }

         var policies = await queryPermitTypePolicies
             .OrderBy(policy=> policy.PermitApplicationType)
             .ThenBy(p=>p.FamilyRelationship)
             .ToListAsync(cancellationToken);

         return mapper.Map<List<PermitTypePolicyDto>>(policies);
      } 
   }
}