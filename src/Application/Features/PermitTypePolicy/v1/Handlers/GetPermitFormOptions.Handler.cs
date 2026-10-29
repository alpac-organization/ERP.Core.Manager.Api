using AutoMapper;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Handlers
{
   public class GetPermitFormOptionsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper) : BaseValidatorHandler<GetPermitFormOptionsQuery, PermitFormOptionsDto>(_unitOfWork, _errorManager)
   {
      public override async Task<PermitFormOptionsDto> Handle(GetPermitFormOptionsQuery request, CancellationToken cancellationToken)
      {
         var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);
         if (!access.IsSuccess)
         {
            return access.ErrorResponse!;
         }

         var types = Enum.GetValues<PermitApplicationType>();
         var scopes = Enum.GetValues<PermitBeneficiaryScope>()
             .Where(scope => scope != PermitBeneficiaryScope.None);

         var relationships = Enum.GetValues<FamilyRelationship>();

         return new PermitFormOptionsDto
         {
            PermitTypes = _mapper.Map<List<EnumOptionsDtos>>(types),
            BeneficiaryScopes = _mapper.Map<List<EnumOptionsDtos>>(scopes),
            FamilyRelationships = _mapper.Map<List<EnumOptionsDtos>>(relationships)
         };
      }
   }   
}