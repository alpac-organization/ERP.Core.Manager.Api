using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Application.Commons.Interfaces.AWS;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Helpers;
using ERP.Core.Manager.Api.Application.Commons.Mappings;
using ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Handlers
{
   public class RegisterSanctionHandler(
      IUnitOfWork _unitOfWork,
      IErrorManager _errorManager,
      ILogger<RegisterSanctionHandler> _logger,
      IS3StorageService _s3StorageService)
      : BaseValidatorHandler<RegisterSanctionCommand, bool>(_unitOfWork, _errorManager)
   {
      public override async Task<bool> Handle(RegisterSanctionCommand request, CancellationToken cancellationToken)
      {
         var access = await ValidateAccessAsync(
            request.UserId, request.CompanyId, request.ModuleCode, cancellationToken);

         if (!access.IsSuccess) return access.ErrorResponse!;

         _logger.LogInformation("🚩Iniciando proceso de validación de registro de sanción disciplinaria");

         if (request.SanctionType == SanctionType.Suspension
             && !HaveValidSuspensionRange(request))
         {
            return _errorManager.ThrowBadRequest<bool>(
               "La suspensión debe durar entre 3 y 7 días.",
               "ERP:InvalidOperation");
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
            return _errorManager.ThrowBadRequest<bool>(
               "La falta disciplinaria no existe o no pertenece a la empresa.",
               "ERP:NotFound");
         }

         if (fault.ParentFaultId.HasValue)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "Esta falta ya fue absorbida por una falta escalada. Debe sancionar la falta padre.",
               "ERP:InvalidOperation");
         }

         if (fault.Status == FaultStatus.Prescribed)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "No se puede sancionar una falta prescrita.",
               "ERP:InvalidOperation");
         }

         if (fault.Status == FaultStatus.SanctionApplied)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "La falta ya tiene una sanción aplicada.",
               "ERP:InvalidOperation");
         }

         var hasActiveSanction = await _unitOfWork.DisciplinarySanctions.Entities
            .AnyAsync(s =>
               s.FaultId == request.FaultId
               && s.DeletedAt == null
               && s.SanctionStatus == SanctionStatus.Applied,
               cancellationToken);

         if (hasActiveSanction)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "Ya existe una sanción aplicada para esta falta.",
               "ERP:InvalidOperation");
         }

         _logger.LogInformation("🚩Iniciando registro de sanción disciplinaria para falta {FaultId}", request.FaultId);

         var evidenceJson = await DisciplinaryEvidenceHelper.BuildEvidenceJsonAsync(
            _s3StorageService,
            request.Evidences,
            "DisciplinarySanctions",
            cancellationToken);

         var sanction = request.ToDisciplinarySanctionEntity(evidenceJson);
         await _unitOfWork.DisciplinarySanctions.RegisterDisciplinarySanction(sanction);

         fault.Status = FaultStatus.SanctionApplied;
         await _unitOfWork.DisciplinaryFaults.UpdateAsync(fault);

         await _unitOfWork.SaveChangesAsync(cancellationToken);

         _logger.LogInformation("✅ Se agregó con éxito el registro de sanción disciplinaria.");

         return true;
      }

      private static bool HaveValidSuspensionRange(RegisterSanctionCommand command)
      {
         if (!command.SuspensionStartDate.HasValue || !command.SuspensionEndDate.HasValue)
            return false;

         var days = command.SuspensionEndDate.Value.DayNumber
            - command.SuspensionStartDate.Value.DayNumber + 1;

         return days is >= 3 and <= 7
            && command.SuspensionEndDate >= command.SuspensionStartDate;
      }
   }
}
