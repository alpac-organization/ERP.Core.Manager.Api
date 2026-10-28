using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Handlers
{
   public class CancelFaultHandler(
      IUnitOfWork _unitOfWork,
      IErrorManager _errorManager,
      ILogger<CancelFaultHandler> _logger)
      : BaseValidatorHandler<CancelFaultCommand, Unit>(_unitOfWork, _errorManager)
   {
      public override async Task<Unit> Handle(CancelFaultCommand request, CancellationToken cancellationToken)
      {
         _logger.LogInformation("🚩Iniciando anulación de falta disciplinaria {FaultId}", request.FaultId);

         var access = await ValidateAccessAsync(
            request.UserId,
            request.CompanyId,
            request.ModuleCode,
            cancellationToken);

         if (!access.IsSuccess)
         {
            return access.ErrorResponse!;
         }

         if (access.Role?.RoleType is not (RoleType.Administrator or RoleType.Manager))
         {
            return _errorManager.ThrowUnauthorized<Unit>(
               "Solo el Gerente General, Gerente de Recursos Humanos o Jefe Inmediato pueden anular faltas disciplinarias.",
               "ERP:INVALID_ACCESS");
         }

         var fault = await _unitOfWork.DisciplinaryFaults.Entities
            .Include(f => f.Collaborator)
            .FirstOrDefaultAsync(f =>
               f.Id == request.FaultId
               && f.DeletedAt == null
               && f.Collaborator.CompanyId == request.CompanyId,
               cancellationToken);

         if (fault is null)
         {
            return _errorManager.ThrowBadRequest<Unit>(
               "La falta disciplinaria no existe o ya fue anulada.",
               "ERP:NotFound");
         }

         if (fault.Status == FaultStatus.SanctionApplied)
         {
            return _errorManager.ThrowBadRequest<Unit>(
               "No se puede anular una falta que ya tiene sanción aplicada.",
               "ERP:InvalidOperation");
         }

         if (fault.Status == FaultStatus.Prescribed)
         {
            return _errorManager.ThrowBadRequest<Unit>(
               "No se puede anular una falta prescrita.",
               "ERP:InvalidOperation");
         }

         fault.DeletedAt = DateTime.UtcNow;

         await _unitOfWork.DisciplinaryFaults.UpdateAsync(fault);
         await _unitOfWork.SaveChangesAsync(cancellationToken);

         _logger.LogInformation("✅ Falta disciplinaria {FaultId} anulada (DeletedAt)", request.FaultId);

         return Unit.Value;
      }
   }
}
