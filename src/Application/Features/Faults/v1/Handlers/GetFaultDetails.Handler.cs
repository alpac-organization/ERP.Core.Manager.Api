using AutoMapper;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Queries;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Handlers
{
   public class GetFaultDetailsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper)
      : BaseValidatorHandler<GetFaultDetailsQuery, FaultDetailDto>(_unitOfWork, _errorManager)
   {
      public override async Task<FaultDetailDto> Handle(GetFaultDetailsQuery request, CancellationToken cancellationToken)
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

         var faultDetails = await _unitOfWork.DisciplinaryFaults.Entities
            .AsNoTracking()
            .Include(df => df.Collaborator)
            .Include(df => df.FaultCatalog)
            .Include(df => df.ParentFault)
            .Include(df => df.User)            
            .FirstOrDefaultAsync(df =>
               df.Id == request.FaultId &&
               df.Collaborator.CompanyId == request.CompanyId &&
               df.DeletedAt == null,
               cancellationToken);

         if (faultDetails is null)
         {
            return _errorManager.ThrowBadRequest<FaultDetailDto>(
               "Detalle de falta disciplinaria de colaborador no fue encontrada.",
               "ERP:NotFound");
         }

         return _mapper.Map<FaultDetailDto>(faultDetails);
      }
   }
}