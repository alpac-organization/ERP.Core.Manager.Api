using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Core.Manager.Api.Application.Features.Terminations.v1.Handlers
{
   public class CloseTerminationHandler(
      IUnitOfWork _unitOfWork,
      IErrorManager _errorManager,
      ILogger<CloseTerminationHandler> _logger)
      : BaseValidatorHandler<CloseTerminationCommand, bool>(_unitOfWork, _errorManager)
   {
      public override async Task<bool> Handle(
         CloseTerminationCommand request,
         CancellationToken cancellationToken)
      {
         var access = await ValidateAccessAsync(
            request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

         if (!access.IsSuccess)
            return access.ErrorResponse;

         if (access.Role!.RoleType is not RoleType.Administrator)
         {
            return _errorManager.ThrowUnauthorized<bool>(
               "Solo Recursos Humanos puede cerrar la baja de un colaborador.",
               "ERP:INVALID_ACCESS");
         }

         _logger.LogInformation(
            "Iniciando cierre de baja. TerminationId={TerminationId}",
            request.TerminationId);

         var termination = await _unitOfWork.CollaboratorTerminations.Entities
            .Include(t => t.Collaborator)
               .ThenInclude(c => c.WorkingInformation)
            .Include(t => t.DisciplinarySanction)
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
               "Solo se puede cerrar un expediente de baja en estado pendiente.",
               "ERP:InvalidOperation");
         }

         var collaborator = termination.Collaborator;

         if (collaborator.WorkingInformation is null)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "El colaborador no tiene información laboral registrada.",
               "ERP:NotFound");
         }

         if (termination.TerminationType == TerminationType.JustifiedDismissal)
         {
            var hasSanctionEvidence = termination.DisciplinarySanction is not null
               && !string.IsNullOrWhiteSpace(termination.DisciplinarySanction.Evidence)
               && termination.DisciplinarySanction.Evidence != "{}";
            var hasTerminationEvidence = !string.IsNullOrWhiteSpace(termination.Evidence)
               && termination.Evidence != "{}";

            if (!hasSanctionEvidence && !hasTerminationEvidence)
            {
               return _errorManager.ThrowBadRequest<bool>(
                  "Para cerrar un despido justificado debe existir evidencia (autorización MITRAB u otra documentación).",
                  "ERP:InvalidOperation");
            }
         }

         var endDate = termination.DepartureDate.ToDateTime(TimeOnly.MinValue);

         termination.TerminationStatus = TerminationProcessStatus.Closed;
         termination.ClosedBy = request.UserId;
         termination.ClosedAt = DateTime.UtcNow;

         collaborator.Status = CollaboratorStatus.Terminated;
         collaborator.HasBeenFired = true;
         collaborator.WorkingInformation.DepartureDate = termination.DepartureDate;

         var activeSalaries = await _unitOfWork.Salaries.Entities
            .Where(s => s.CollaboratorId == collaborator.Id && s.EndDate == null)
            .ToListAsync(cancellationToken);

         foreach (var salary in activeSalaries)
         {
            salary.EndDate = endDate;
            await _unitOfWork.Salaries.UpdateAsync(salary);
         }

         var activePositions = await _unitOfWork.WorkPositionHistories.Entities
            .Where(h => h.CollaboratorId == collaborator.Id && h.EndDate == null)
            .ToListAsync(cancellationToken);

         foreach (var position in activePositions)
         {
            position.EndDate = endDate;
            await _unitOfWork.WorkPositionHistories.UpdateAsync(position);
         }

         var pendingPermits = await _unitOfWork.PermitApplications.Entities
            .Where(p =>
               p.CollaboratorId == collaborator.Id
               && p.Status == PermitApplicationStatus.Pending)
            .ToListAsync(cancellationToken);

         foreach (var permit in pendingPermits)
         {
            permit.Status = PermitApplicationStatus.Cancelled;
            await _unitOfWork.PermitApplications.UpdateAsync(permit);
         }

         await _unitOfWork.CollaboratorTerminations.UpdateAsync(termination);
         await _unitOfWork.Collaborators.UpdateAsync(collaborator);
         await _unitOfWork.SaveChangesAsync(cancellationToken);

         _logger.LogInformation(
            "Baja cerrada. TerminationId={TerminationId}, CollaboratorId={CollaboratorId}. Colaborador listo para liquidación (DeletedAt sigue null).",
            termination.Id,
            collaborator.Id);

         return true;
      }
   }
}
