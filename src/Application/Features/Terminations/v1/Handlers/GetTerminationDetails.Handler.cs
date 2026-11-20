using AutoMapper;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Queries;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Application.Features.Terminations.v1.Handlers
{
   public class GetTerminationDetailsHandler(
      IUnitOfWork _unitOfWork,
      IErrorManager _errorManager,
      IMapper _mapper)
      : BaseValidatorHandler<GetTerminationDetailsQuery, TerminationDetailDto>(_unitOfWork, _errorManager)
   {
      public override async Task<TerminationDetailDto> Handle(
         GetTerminationDetailsQuery request,
         CancellationToken cancellationToken)
      {
         var access = await ValidateAccessAsync(
            request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

         if (!access.IsSuccess)
            return access.ErrorResponse!;

         var termination = await _unitOfWork.CollaboratorTerminations.Entities
            .AsNoTracking()
            .Include(t => t.Collaborator)
            .Include(t => t.RegisteredByUser)
            .Include(t => t.ClosedByUser)
            .FirstOrDefaultAsync(t =>
               t.Id == request.TerminationId
               && t.DeletedAt == null
               && t.Collaborator.CompanyId == request.CompanyId,
               cancellationToken);

         if (termination is null)
         {
            return _errorManager.ThrowBadRequest<TerminationDetailDto>(
               "El expediente de baja no existe o no pertenece a la empresa.",
               "ERP:NotFound");
         }

         return _mapper.Map<TerminationDetailDto>(termination);
      }
   }
}
