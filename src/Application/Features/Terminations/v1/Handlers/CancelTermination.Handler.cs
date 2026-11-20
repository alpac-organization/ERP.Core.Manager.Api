using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Core.Manager.Api.Application.Features.Terminations.v1.Handlers
{
   public class CancelTerminationHandler(
      IUnitOfWork _unitOfWork,
      IErrorManager _errorManager,
      ILogger<CancelTerminationHandler> _logger)
      : BaseValidatorHandler<CancelTerminationCommand, bool>(_unitOfWork, _errorManager)
   {
      public override async Task<bool> Handle(
         CancelTerminationCommand request,
         CancellationToken cancellationToken)
      {
         var access = await ValidateAccessAsync(
            request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

         if (!access.IsSuccess)
            return access.ErrorResponse;

         if (access.Role!.RoleType is not RoleType.Administrator)
         {
            return _errorManager.ThrowUnauthorized<bool>(
               "Solo Recursos Humanos puede anular la baja de un colaborador.",
               "ERP:INVALID_ACCESS");
         }

         _logger.LogInformation(
            "Iniciando anulación de baja. TerminationId={TerminationId}",
            request.TerminationId);

         var termination = await _unitOfWork.CollaboratorTerminations.Entities
            .Include(t => t.Collaborator)
            .FirstOrDefaultAsync(t =>
               t.Id == request.TerminationId
               && t.DeletedAt == null
               && t.Collaborator.CompanyId == request.CompanyId,
               cancellationToken);

         if (termination is null)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "El expediente de baja no existe o no pertenece a la empresa.",
               "ERP:NotFound");
         }

         if (termination.TerminationStatus != TerminationProcessStatus.Pending)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "Solo se puede anular un expediente de baja en estado pendiente.",
               "ERP:InvalidOperation");
         }

         termination.TerminationStatus = TerminationProcessStatus.Cancelled;
         termination.CancellationReason = request.CancellationReason.Trim();

         await _unitOfWork.CollaboratorTerminations.UpdateAsync(termination);
         await _unitOfWork.SaveChangesAsync(cancellationToken);

         _logger.LogInformation(
            "Baja anulada. TerminationId={TerminationId}",
            termination.Id);

         return true;
      }
   }
}
