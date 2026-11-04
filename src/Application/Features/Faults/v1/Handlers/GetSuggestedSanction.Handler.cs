using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Queries;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Handlers
{
   public class GetSuggestedSanctionHandler(
      IUnitOfWork _unitOfWork,
      IErrorManager _errorManager,
      IDisciplinaryEscalation _disciplinaryEscalation)
      : BaseValidatorHandler<GetSuggestedSanctionQuery, SuggestedSanctionDto>(_unitOfWork, _errorManager)
   {
      private const int MaxEscalationDepth = 2; // Tope de saltos de profundidad: leve → grave → muy grave

      public override async Task<SuggestedSanctionDto> Handle(
         GetSuggestedSanctionQuery request,
         CancellationToken cancellationToken)
      {
         var access = await ValidateAccessAsync(
            request.UserId, request.CompanyId, request.ModuleCode, cancellationToken);

         if (!access.IsSuccess)
            return access.ErrorResponse!;

         var fault = await _unitOfWork.DisciplinaryFaults.Entities
            .AsNoTracking()
            .Include(f => f.Collaborator)
            .FirstOrDefaultAsync(f =>
               f.Id == request.FaultId
               && f.DeletedAt == null
               && f.Collaborator.CompanyId == request.CompanyId,
               cancellationToken);

         if (fault is null)
         {
            return _errorManager.ThrowBadRequest<SuggestedSanctionDto>(
               "La falta disciplinaria no existe.",
               "ERP:NotFound");
         }

         var requestedFaultId = fault.Id;
         var isCoveredByEscalation = fault.ParentFaultId.HasValue;
         DisciplinaryFault targetFault = fault;

         if (isCoveredByEscalation)
         {
            var rootFault = await ResolveSanctionRootAsync(
               fault, request.CompanyId, cancellationToken);

            if (rootFault is null)
            {
               return _errorManager.ThrowBadRequest<SuggestedSanctionDto>(
                  "Esta falta está vinculada a una reincidencia que no se encontró o fue eliminada.",
                  "ERP:NotFound");
            }

            if (rootFault.Status == FaultStatus.SanctionApplied)
            {
               return _errorManager.ThrowBadRequest<SuggestedSanctionDto>(
                  "La falta escalada ya tiene una sanción aplicada.",
                  "ERP:InvalidOperation");
            }

            if (rootFault.Status == FaultStatus.Prescribed)
            {
               return _errorManager.ThrowBadRequest<SuggestedSanctionDto>(
                  "No se puede sugerir sanción para una falta prescrita.",
                  "ERP:InvalidOperation");
            }

            targetFault = rootFault;
         }
         else
         {
            if (fault.Status == FaultStatus.Prescribed)
            {
               return _errorManager.ThrowBadRequest<SuggestedSanctionDto>(
                  "No se puede sugerir sanción para una falta prescrita.",
                  "ERP:InvalidOperation");
            }

            if (fault.Status == FaultStatus.SanctionApplied)
            {
               return _errorManager.ThrowBadRequest<SuggestedSanctionDto>(
                  "La falta ya tiene una sanción aplicada.",
                  "ERP:InvalidOperation");
            }
         }

         var (monthStart, monthEnd) = _disciplinaryEscalation.GetMonthBoundaries(targetFault.OccurrenceDate);

         var countOccurrenceInMonth = await _unitOfWork.DisciplinaryFaults.Entities
            .AsNoTracking()
            .CountAsync(f =>
               f.DeletedAt == null
               && f.CollaboratorId == targetFault.CollaboratorId
               && f.Severity == targetFault.Severity
               && f.ParentFaultId == null
               && f.OccurrenceDate >= monthStart
               && f.OccurrenceDate < monthEnd,
               cancellationToken);

         var (suggested, allowed) = Suggest(targetFault.Severity, countOccurrenceInMonth);

         return new SuggestedSanctionDto
         {
            FaultId = requestedFaultId,
            SanctionTargetFaultId = targetFault.Id,
            ParentFaultId = isCoveredByEscalation ? targetFault.Id : null,
            ParentFaultCode = isCoveredByEscalation ? targetFault.Code : null,
            IsCoveredByEscalation = isCoveredByEscalation,
            Severity = targetFault.Severity,
            CountOccurrenceInMonth = countOccurrenceInMonth,
            SuggestedSanctionType = suggested,
            AllowedSanctionTypes = allowed,
         };
      }

      /// <summary>
      /// Sube la cadena ParentFaultId hasta la raíz sancionable
      /// (p. ej. leve → grave → muy grave).
      /// </summary>
      private async Task<DisciplinaryFault?> ResolveSanctionRootAsync(
         DisciplinaryFault fault,
         Guid companyId,
         CancellationToken cancellationToken)
      {
         var current = fault;
         var depth = 0;

         while (current.ParentFaultId.HasValue)
         {
            if (++depth > MaxEscalationDepth)
               return null;

            var parent = await _unitOfWork.DisciplinaryFaults.Entities
               .AsNoTracking()
               .Include(f => f.Collaborator)
               .FirstOrDefaultAsync(f =>
                  f.Id == current.ParentFaultId.Value
                  && f.DeletedAt == null
                  && f.Collaborator.CompanyId == companyId,
                  cancellationToken);

            if (parent is null)
               return null;

            current = parent;
         }

         return current;
      }

      private static (SanctionType Suggested, List<SanctionType> Allowed) Suggest(
         FaultSeverity severity,
         int occurrenceInMonth)
      {
         return severity switch
         {
            FaultSeverity.Minor when occurrenceInMonth <= 1 =>
               (SanctionType.VerbalWarning, [SanctionType.VerbalWarning, SanctionType.Memorandum]),

            FaultSeverity.Minor =>
               (SanctionType.Memorandum, [SanctionType.Memorandum]),

            FaultSeverity.Serious when occurrenceInMonth <= 1 =>
               (SanctionType.Memorandum, [SanctionType.Memorandum, SanctionType.Suspension]),

            FaultSeverity.Serious =>
               (SanctionType.Suspension, [SanctionType.Suspension]),

            FaultSeverity.VerySerious =>
               (SanctionType.CommitmentLetter, [SanctionType.CommitmentLetter, SanctionType.Termination]),

            _ => (SanctionType.VerbalWarning, [SanctionType.VerbalWarning])
         };
      }
   }
}
