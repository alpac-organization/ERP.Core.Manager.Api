using AutoMapper;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Queries;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Handlers
{
   public class GetSanctionDetailsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper)
      : BaseValidatorHandler<GetSanctionDetailsQuery, SanctionDetailDto>(_unitOfWork, _errorManager)
   {
      public override async Task<SanctionDetailDto> Handle(
         GetSanctionDetailsQuery request,
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

         var sanction = await _unitOfWork.DisciplinarySanctions.Entities
            .AsNoTracking()
            .Include(s => s.Fault)
               .ThenInclude(f => f.Collaborator)
            .Include(s => s.Fault)
               .ThenInclude(f => f.FaultCatalog)
            .FirstOrDefaultAsync(s =>
               s.Id == request.SanctionId
               && s.DeletedAt == null
               && s.Fault.DeletedAt == null
               && s.Fault.Collaborator.CompanyId == request.CompanyId,
               cancellationToken);

         if (sanction is null)
         {
            return _errorManager.ThrowBadRequest<SanctionDetailDto>(
               "Detalle de sanción disciplinaria no fue encontrado.",
               "ERP:NotFound");
         }

         var detail = _mapper.Map<SanctionDetailDto>(sanction);

         var issuedByUser = await _unitOfWork.Users.Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == sanction.IssuedBy && u.DeletedAt == null, cancellationToken);

         if (issuedByUser is not null)
         {
            detail.IssuedByUser = _mapper.Map<RegisterUserDto>(issuedByUser);
         }

         return detail;
      }
   }
}
