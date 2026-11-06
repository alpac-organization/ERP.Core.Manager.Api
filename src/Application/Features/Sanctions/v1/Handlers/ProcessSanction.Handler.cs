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

         // TODO: Implementar lo siguiente:
         // 1. Revisar si la sancion es de tipo suspensión. ✅
         // 2. Si es suspension calcular dias * (salario / 30). ❌
         // 3. Agregar registro a historial de deducciones. ❌
         // 4. Agregar registro de deducciones. ❌
         // 5. Sanction + Totales. ❌
         // 6. Agregar registro Ciclo de Nomina (Posiblemente Nomina). ❌




         var suspensionResult = await ProcessSuspension(fault, sanction, cancellationToken);

         if (!suspensionResult) return suspensionResult;

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

      private async Task<bool> ProcessSuspension(
         DisciplinaryFault fault,
         DisciplinarySanction sanction,
         CancellationToken cancellationToken)
      {
         if (sanction.SanctionType == SanctionType.Suspension)
         {
            var collaborator = await _unitOfWork.Collaborators.Entities
               .Include(c => c.WorkingInformation)
               .Where(c => c.Id == fault.CollaboratorId && !c.HasBeenFired && c.DeletedAt == null)
               .FirstOrDefaultAsync(cancellationToken);

            if (collaborator is null || collaborator.WorkingInformation is null)
            {
               return _errorManager.ThrowBadRequest<bool>(
                  "No se encontró al colaborador o su información laboral.",
                  "ERP:NotFound");
            }

            /* var payrollActive = await _unitOfWork.Payrolls.Entities
               .Where(p => p.Status == PayrollStatus.Progress && p.PayrollType == PayrollType.Ordinary)
               .Where(p => p.BranchId == collaborator.WorkingInformation.BranchId)
               .FirstOrDefaultAsync(cancellationToken);

            if (payrollActive is null)
            {
               return _errorManager.ThrowBadRequest<bool>(
                  "No hay una nómina ordinaria en progreso para la sucursal del colaborador.",
                  "ERP:PeriodNotExist");
            } */

            if (!sanction.SuspensionEndDate.HasValue || !sanction.SuspensionStartDate.HasValue)
            {
               return _errorManager.ThrowBadRequest<bool>(
                  "La suspensión no tiene rango de fechas válido.",
                  "ERP:InvalidOperation");
            }

            var amountDays = sanction.SuspensionEndDate.Value.DayNumber -
                             sanction.SuspensionStartDate.Value.DayNumber + 1;

            if (amountDays is < 3 or > 7)
            {
               return _errorManager.ThrowBadRequest<bool>(
                  "La suspensión debe durar entre 3 y 7 días.",
                  "ERP:InvalidOperation");
            }

            if (!sanction.PayrollId.HasValue)
            {
               return _errorManager.ThrowBadRequest<bool>(
                  "No se encontro la información de nomina asociada a la sanción.",
                  "ERP:InvalidOperation");
            }

            var applied = await _deductionsServices.ApplySanctionDeduction(
               collaborator, amountDays, sanction.PayrollId.Value);

            if (!applied)
            {
               return _errorManager.ThrowBadRequest<bool>(
                  "No se pudo aplicar la deducción por sanción.",
                  "ERP:01");
            }
         }

         return true;
      }
   }
}
