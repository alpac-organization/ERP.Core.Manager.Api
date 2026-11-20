using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Application.Commons.Interfaces.AWS;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Helpers;
using ERP.Core.Manager.Api.Application.Commons.Mappings;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Core.Manager.Api.Application.Features.Terminations.v1.Handlers
{
   public class RegisterTerminationHandler(
      IUnitOfWork _unitOfWork,
      IErrorManager _errorManager,
      ILogger<RegisterTerminationHandler> _logger,
      IS3StorageService _s3StorageService)
      : BaseValidatorHandler<RegisterTerminationCommand, bool>(_unitOfWork, _errorManager)
   {
      public override async Task<bool> Handle(
         RegisterTerminationCommand request,
         CancellationToken cancellationToken)
      {
         var access = await ValidateAccessAsync(
            request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

         if (!access.IsSuccess) return access.ErrorResponse;

         if (access.Role!.RoleType is not RoleType.Administrator)
         {
            return _errorManager.ThrowUnauthorized<bool>(
               "Solo Recursos Humanos puede registrar la baja de un colaborador.",
               "ERP:INVALID_ACCESS");
         }

         _logger.LogInformation(
            "Iniciando registro de baja para el colaborador {CollaboratorId}",
            request.CollaboratorId);

         var collaborator = await _unitOfWork.Collaborators.Entities
            .AsNoTracking()
            .Include(c => c.WorkingInformation)
            .Where(c =>
               c.Id == request.CollaboratorId
               && c.CompanyId == request.CompanyId
               && c.DeletedAt == null
               && !c.HasBeenFired
               && c.Status != CollaboratorStatus.Terminated
               && c.Status != CollaboratorStatus.Inactive)
            .FirstOrDefaultAsync(cancellationToken);

         if (collaborator is null)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "El colaborador no existe, no pertenece a la empresa o no tiene una relación laboral activa.",
               "ERP:NotFound");
         }

         if (collaborator.WorkingInformation is null)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "El colaborador no tiene información laboral registrada.",
               "ERP:NotFound");
         }

         var hasPendingTermination = await _unitOfWork.CollaboratorTerminations.Entities
            .AnyAsync(t =>
               t.CollaboratorId == collaborator.Id
               && t.TerminationStatus == TerminationProcessStatus.Pending
               && t.DeletedAt == null,
               cancellationToken);

         if (hasPendingTermination)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "Ya existe un expediente de baja pendiente para este colaborador.",
               "ERP:InvalidOperation");
         }

         if (request.DepartureDate < collaborator.WorkingInformation.EntryDate)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "La fecha efectiva de terminación no puede ser anterior a la fecha de ingreso del colaborador.",
               "ERP:InvalidOperation");
         }

         if (request.SanctionId.HasValue)
         {
            var sanction = await _unitOfWork.DisciplinarySanctions.Entities
               .AsNoTracking()
               .Include(s => s.Fault)
               .FirstOrDefaultAsync(s =>
                  s.Id == request.SanctionId.Value
                  && s.DeletedAt == null
                  && s.Fault.DeletedAt == null
                  && s.Fault.CollaboratorId == collaborator.Id,
                  cancellationToken);

            if (sanction is null)
            {
               return _errorManager.ThrowBadRequest<bool>(
                  "La sanción disciplinaria no existe o no pertenece al colaborador.",
                  "ERP:NotFound");
            }

            if (sanction.SanctionStatus != SanctionStatus.Applied)
            {
               return _errorManager.ThrowBadRequest<bool>(
                  "Solo se puede enlazar una sanción disciplinaria en estado aplicada.",
                  "ERP:InvalidOperation");
            }

            if (sanction.SanctionType != SanctionType.Termination)
            {
               return _errorManager.ThrowBadRequest<bool>(
                  "La sanción enlazada debe ser de tipo terminación de contrato.",
                  "ERP:InvalidOperation");
            }

            var sanctionAlreadyLinked = await _unitOfWork.CollaboratorTerminations.Entities
               .AnyAsync(t =>
                  t.SanctionId == sanction.Id
                  && t.DeletedAt == null,
                  cancellationToken);

            if (sanctionAlreadyLinked)
            {
               return _errorManager.ThrowBadRequest<bool>(
                  "La sanción disciplinaria ya está asociada a otro expediente de baja.",
                  "ERP:InvalidOperation");
            }

            if (request.TerminationType == TerminationType.JustifiedDismissal)
            {
               var hasSanctionEvidence = !string.IsNullOrWhiteSpace(sanction.Evidence)
                  && sanction.Evidence != "{}";
               var hasRequestEvidence = request.Evidences is { Count: > 0 };

               if (!hasSanctionEvidence && !hasRequestEvidence)
               {
                  return _errorManager.ThrowBadRequest<bool>(
                     "Para un despido justificado debe registrar evidencia (autorización MITRAB u otra documentación).",
                     "ERP:InvalidOperation");
               }
            }
         }

         var evidenceJson = await EvidenceHelper.BuildEvidenceJsonAsync(
            _s3StorageService,
            request.Evidences,
            "Terminations",
            cancellationToken);

         var termination = request.ToTerminationEntity(evidenceJson);
         await _unitOfWork.CollaboratorTerminations.RegisterTermination(termination);
         await _unitOfWork.SaveChangesAsync(cancellationToken);

         _logger.LogInformation(
            "Expediente de baja registrado en pendiente. CollaboratorId={CollaboratorId}, TerminationId={TerminationId}",
            request.CollaboratorId,
            termination.Id);

         return true;
      }
   }
}
