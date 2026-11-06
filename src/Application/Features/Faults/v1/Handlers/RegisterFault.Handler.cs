using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Application.Commons.Interfaces.AWS;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Application.Commons.Interfaces.Services;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Helpers;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Mappings;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Handlers
{
   public class RegisterFaultHandler(
      IUnitOfWork _unitOfWork,
      IErrorManager _errorManager,
      ILogger<RegisterFaultHandler> _logger,
      IDisciplinaryEscalation _disciplinaryEscalation,
      IS3StorageService _s3StorageService,
      ICodeGenerator _codeGenerator)
      : BaseValidatorHandler<RegisterFaultCommand, bool>(_unitOfWork, _errorManager)
   {
      public override async Task<bool> Handle(RegisterFaultCommand request, CancellationToken cancellationToken)
      {
         var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode, cancellationToken);

         if (!access.IsSuccess) return access.ErrorResponse!;

         if (access.Role?.RoleType is not (RoleType.Administrator or RoleType.Manager))
         {
            return _errorManager.ThrowUnauthorized<bool>(
               "Solo el Gerente General, Gerente de Recursos Humanos o Jefe Inmediato pueden registrar faltas disciplinarias.",
               "ERP:INVALID_ACCESS");
         }

         _logger.LogInformation("🚩Iniciando proceso de validación de registro de faltas disciplinaria");

         var faults = request.Faults;

         var faultsCatalogIds = faults.Select(f => f.FaultCatalogId).Distinct().ToList();

         var existingFaultCatalogs = await _unitOfWork.FaultCatalogs.Entities
            .Where(fc => faultsCatalogIds.Contains(fc.Id)
                     && fc.IsActive
                     && fc.DeletedAt == null
                     && fc.CompanyId == request.CompanyId)
            .ToListAsync(cancellationToken);

         if (existingFaultCatalogs.Count != faultsCatalogIds.Count)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "Uno o más catalogos de falta disciplinaria no existen o no están activos",
               "ERP:NOT_FOUND");
         }

         var companyCode = await _unitOfWork.Companies.Entities
            .AsNoTracking()
            .Where(c => c.IsActive && c.DeletedAt == null && c.Id == request.CompanyId)
            .Select(c => c.Code)
            .FirstOrDefaultAsync(cancellationToken);

         if (existingFaultCatalogs.Any(fc => ExcludedFaultCatalogs.IsExcluded(companyCode, fc.Code)))
         {
            return _errorManager.ThrowBadRequest<bool>(
               "Una o más faltas seleccionadas se procesan de forma automática (marcación biométrica) y no pueden registrarse manualmente.",
               "ERP:InvalidOperation");
         }

         var collaboratorIds = faults.Select(f => f.CollaboratorId).Distinct().ToList();

         var existingCollaboratorIds = await _unitOfWork.Collaborators.Entities
            .Where(c => collaboratorIds.Contains(c.Id)
                        && c.CompanyId == request.CompanyId
                        && c.Status != CollaboratorStatus.Inactive)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

         if (existingCollaboratorIds.Count != collaboratorIds.Count)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "Uno o más colaboradores no existen o no están activos en la empresa especificada.",
               "ERP:NotFound");
         }

         var parentIds = faults
            .Where(f => f.ParentFaultId.HasValue)
            .Select(f => f.ParentFaultId!.Value)
            .Distinct()
            .ToList();

         if (parentIds.Count > 0)
         {
            var existingParentIds = await _unitOfWork.DisciplinaryFaults.Entities
               .Where(f => parentIds.Contains(f.Id) && f.DeletedAt == null)
               .Select(f => f.Id)
               .ToListAsync(cancellationToken);

            if (existingParentIds.Count != parentIds.Count)
            {
               return _errorManager.ThrowBadRequest<bool>(
                  "Una o más faltas padre no existen.",
                  "ERP:NotFound");
            }
         }

         _logger.LogInformation("🚩Iniciando proceso de registro de faltas disciplinaria");

         var faultsCatalogDictionary = existingFaultCatalogs.ToDictionary(fc => fc.Id, fc => fc.FaultSeverity);
         var registeredFaults = new List<DisciplinaryFault>();

         foreach (var fault in faults)
         {
            if (!faultsCatalogDictionary.TryGetValue(fault.FaultCatalogId, out var faultSeverity))
            {
               return _errorManager.ThrowBadRequest<bool>(
                  "Uno o más catalogos de falta disciplinaria no existen o no están activos",
                  "ERP:NOT_FOUND");
            }

            var evidenceJson = await DisciplinaryEvidenceHelper.BuildEvidenceJsonAsync(
               _s3StorageService,
               fault.Evidences,
               "DisciplinaryFaults",
               cancellationToken);

            await _unitOfWork.ExecuteWithUniqueCodeRetryAsync(async token =>
            {
               var (IsSuccess, faultCode) = await _codeGenerator.GenerateUniqueDisciplinaryFaultCodeAsync(
                  request.CompanyId,
                  fault.OccurrenceDate,
                  token
               );

               if (!IsSuccess || string.IsNullOrWhiteSpace(faultCode))
               {
                  _errorManager.ThrowBadRequest<bool>(
                    "No se pudo generar el código falta disciplinaria.",
                    "ERP:FAULT_CODE_GENERATION_FAILED");
                  return;
               }

               var faultDisciplinary = fault.ToDisciplinaryFaultEntity(
                  faultCode, faultSeverity, request.UserId, evidenceJson
               );

               await _unitOfWork.DisciplinaryFaults.RegisterDisciplinaryFault(faultDisciplinary);
               await _unitOfWork.SaveChangesAsync(token);
               registeredFaults.Add(faultDisciplinary);
            }, cancellationToken);
         }

         var escalationTargets = registeredFaults
            .GroupBy(f => new
            {
               f.CollaboratorId,
               f.Severity,
               Month = new DateOnly(f.OccurrenceDate.Year, f.OccurrenceDate.Month, 1)
            })
            .Select(g => g.First());

         foreach (var registeredFault in escalationTargets)
         {
            await _disciplinaryEscalation.ExecuteEscalationProcess(registeredFault, cancellationToken);
         }

         _logger.LogInformation("✅ Se agrego con exito el registro de faltas disciplinarias.");

         return true;
      }
   }
}
