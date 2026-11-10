using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Handlers
{
   public class ProcessSanctionHandler(
      IUnitOfWork _unitOfWork,
      IErrorManager _errorManager,
      ILogger<ProcessSanctionHandler> _logger,
      IDisciplinaryEscalation _disciplinaryEscalation,
      IDeductionsServices _deductionsServices)
      : BaseValidatorHandler<ProcessSanctionCommand, bool>(_unitOfWork, _errorManager)
   {
      public override async Task<bool> Handle(
         ProcessSanctionCommand request,
         CancellationToken cancellationToken)
      {
         var access = await ValidateAccessAsync(
            request.UserId, request.CompanyId, request.ModuleCode, cancellationToken);

         if (!access.IsSuccess) return access.ErrorResponse!;

         var sanction = await _unitOfWork.DisciplinarySanctions.Entities
            .Include(s => s.Fault)
               .ThenInclude(f => f.ParentFault)
            .Include(s => s.Fault)
               .ThenInclude(f => f.Collaborator)
            .FirstOrDefaultAsync(s =>
               s.Id == request.SanctionId
               && s.DeletedAt == null
               && s.Fault.DeletedAt == null
               && s.Fault.Collaborator.CompanyId == request.CompanyId,
               cancellationToken);

         if (sanction is null)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "La sanción disciplinaria no existe o no pertenece a la empresa.",
               "ERP:NotFound");
         }

         if (sanction.SanctionStatus == SanctionStatus.Cancelled)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "La sanción ya está cancelada.",
               "ERP:InvalidOperation");
         }

         if (request.IsApproved)
         {
            return await ApproveAsync(request, sanction, access.Role!.RoleType, cancellationToken);
         }

         return await CancelAsync(request, sanction, access.Role!.RoleType, cancellationToken);
      }

      private async Task<bool> ApproveAsync(
         ProcessSanctionCommand request,
         DisciplinarySanction sanction,
         RoleType roleType,
         CancellationToken cancellationToken)
      {
         // Solo RRHH / Gerente General (Administrator)
         if (roleType is not RoleType.Administrator)
         {
            return _errorManager.ThrowUnauthorized<bool>(
               "Solo Recursos Humanos puede aplicar la sanción.",
               "ERP:INVALID_ACCESS");
         }

         if (sanction.SanctionStatus != SanctionStatus.Pending)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "Solo se puede aplicar una sanción en estado pendiente.",
               "ERP:InvalidOperation");
         }

         var fault = sanction.Fault;

         if (fault.ParentFaultId.HasValue)
         {
            var parentRef = !string.IsNullOrWhiteSpace(fault.ParentFault?.Code)
                ? fault.ParentFault.Code
                : "";
            return _errorManager.ThrowBadRequest<bool>(
               $"Esta falta ya fue absorbida por una falta escalada. Debe sancionar la falta padre: {parentRef}.",
               "ERP:InvalidOperation");
         }

         if (fault.Status == FaultStatus.Prescribed)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "No se puede aplicar una sanción sobre una falta prescrita.",
               "ERP:InvalidOperation");
         }

         if (fault.Status == FaultStatus.Cancelled)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "No se puede aplicar una sanción sobre una falta anulada.",
               "ERP:InvalidOperation");
         }

         _logger.LogInformation(
            "Aplicando sanción disciplinaria {SanctionId} por usuario {UserId}",
            sanction.Id, request.UserId);         

         if (sanction.SanctionType == SanctionType.Suspension &&
            (!request.PayrollId.HasValue || request.PayrollId == Guid.Empty))
         {
            return _errorManager.ThrowBadRequest<bool>(
                  "El id de la nómina es requerido al aplicar una suspensión.",
                  "ERP:InvalidOperation");
         }

         var (suspensionResult, payrollActiveId) = await ProcessSuspension(
            fault, sanction, request.PayrollId ?? Guid.Empty, cancellationToken);

         if (!suspensionResult) return suspensionResult;

         if (payrollActiveId != Guid.Empty) sanction.PayrollId = payrollActiveId;

         sanction.SanctionStatus = SanctionStatus.Applied;
         sanction.ApprovedBy = request.UserId;
         sanction.ApprovedAt = DateTime.UtcNow;

         await _unitOfWork.DisciplinarySanctions.UpdateAsync(sanction);

         fault.Status = FaultStatus.SanctionApplied;
         await _unitOfWork.DisciplinaryFaults.UpdateAsync(fault);
         await _disciplinaryEscalation.MarkLinkedFaultsAsSanctionApplied(fault, cancellationToken);

         await _unitOfWork.SaveChangesAsync(cancellationToken);

         _logger.LogInformation("✅ Sanción {SanctionId} aplicada.", sanction.Id);

         return true;
      }

      private async Task<bool> CancelAsync(
         ProcessSanctionCommand request,
         DisciplinarySanction sanction,
         RoleType roleType,
         CancellationToken cancellationToken)
      {
         if (roleType is not (RoleType.Administrator or RoleType.Manager))
         {
            return _errorManager.ThrowUnauthorized<bool>(
               "Solo el Gerente General, Gerente de Recursos Humanos o Jefe Inmediato pueden cancelar sanciones.",
               "ERP:INVALID_ACCESS");
         }

         if (sanction.SanctionStatus == SanctionStatus.Applied)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "No se puede cancelar una sanción ya aplicada.",
               "ERP:InvalidOperation");
         }

         if (sanction.SanctionStatus != SanctionStatus.Pending)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "Solo se puede cancelar una sanción en estado pendiente.",
               "ERP:InvalidOperation");
         }

         _logger.LogInformation(
            "Cancelando sanción disciplinaria {SanctionId} por usuario {UserId}",
            sanction.Id, request.UserId);

         sanction.SanctionStatus = SanctionStatus.Cancelled;
         sanction.CancellationReason = request.CancellationReason!.Trim();
         await _unitOfWork.DisciplinarySanctions.UpdateAsync(sanction);

         await _unitOfWork.SaveChangesAsync(cancellationToken);

         _logger.LogInformation("✅ Sanción {SanctionId} cancelada.", sanction.Id);
         return true;
      }

      private async Task<(bool, Guid)> ProcessSuspension(
         DisciplinaryFault fault,
         DisciplinarySanction sanction,
         Guid payrollId,
         CancellationToken cancellationToken)
      {
         // Amonestación u otras sanciones: no impactan nómina
         if (sanction.SanctionType != SanctionType.Suspension)
            return (true, Guid.Empty);

         var collaborator = await _unitOfWork.Collaborators.Entities
            .Include(c => c.WorkingInformation)
            .Where(c => c.Id == fault.CollaboratorId && !c.HasBeenFired && c.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);

         if (collaborator is null || collaborator.WorkingInformation is null)
         {
            _errorManager.ThrowBadRequest<bool>(
              "No se encontró al colaborador o su información laboral.",
              "ERP:NotFound");

            return (false, Guid.Empty);
         }

         if (!sanction.SuspensionEndDate.HasValue || !sanction.SuspensionStartDate.HasValue)
         {
            _errorManager.ThrowBadRequest<bool>(
              "La suspensión no tiene rango de fechas válido.",
              "ERP:InvalidOperation");

            return (false, Guid.Empty);
         }

         var amountDays = sanction.SuspensionEndDate.Value.DayNumber -
                          sanction.SuspensionStartDate.Value.DayNumber + 1;

         if (amountDays is < 3 or > 7)
         {
            _errorManager.ThrowBadRequest<bool>(
               "La suspensión debe durar entre 3 y 7 días.",
               "ERP:InvalidOperation");

            return (false, Guid.Empty);
         }

         var (isApplied, payrollActiveId) = await _deductionsServices
            .ApplySanctionDeduction(collaborator, amountDays, payrollId);

         if (!isApplied || payrollActiveId == Guid.Empty)
         {
            _errorManager.ThrowBadRequest<bool>(
               "No se pudo aplicar la deducción por sanción. Verifique nómina activa, salario y registro en nómina ordinaria.",
               "ERP:01");

            return (false, Guid.Empty);
         }

         return (true, payrollActiveId);
      }
   }
}
