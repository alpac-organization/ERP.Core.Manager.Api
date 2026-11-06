using System.Collections.Frozen;
using System.Text.Json;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Application.Commons.Interfaces.Services;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
   public class DisciplinaryEscalationService(
      IUnitOfWork _unitOfWork,
      IErrorManager _errorManager,
      ICodeGenerator _codeGenerator) : IDisciplinaryEscalation
   {
      private const int EscalationThreshold = 3;

      // Catalogo de falta tipo Reincidencias por empresa
      private static readonly FrozenDictionary<(string Alias, FaultSeverity Severity), string> RepeatFaultCodesByCompany =
         new Dictionary<(string, FaultSeverity), string>
         {
            [("ALP", FaultSeverity.Serious)] = "FG-ALP-01",
            [("ALP", FaultSeverity.VerySerious)] = "FMG-ALP-01",

            [("AVS", FaultSeverity.Serious)] = "FG-AVS-01",
            [("AVS", FaultSeverity.VerySerious)] = "FMG-AVS-01",

            [("TMN", FaultSeverity.Serious)] = "FG-TMN-01",
            [("TMN", FaultSeverity.VerySerious)] = "FMG-TMN-01",

            [("AMS", FaultSeverity.Serious)] = "FG-AMS-01",
            [("AMS", FaultSeverity.VerySerious)] = "FMG-AMS-01",

            [("VGM", FaultSeverity.Serious)] = "FG-VGM-01",
            [("VGM", FaultSeverity.VerySerious)] = "FMG-VGM-01",
         }.ToFrozenDictionary();

      // Códigos legales para escalamiento directo de inasistencias injustificadas (3 días en el mes calendario -> Falta Muy Grave)
      private static readonly FrozenDictionary<string, string> AbsenceEscalationCodesByCompany =
         new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
         {
            ["ALP"] = "FMG-ALP-04",
            ["AVS"] = "FMG-AVS-04",
         }.ToFrozenDictionary();


      // Escalamiento de inasistencias injustificadas según normativa legal
      public async Task<DisciplinaryFault?> CheckAndEscalateAbsencesAsync(
         Guid collaboratorId,
         DateOnly occurrenceDate,
         Guid userId,
         CancellationToken cancellationToken)
      {
         var (monthStart, monthEnd) = GetMonthBoundaries(occurrenceDate);

         // 1. Obtener todas las inasistencias aplicadas del colaborador dentro del mismo mes calendario (continuas o discontinuas)
         var appliedMonthAbsences = await _unitOfWork.Absences.Entities
            .Where(a => a.CollaboratorId == collaboratorId
                     && a.Status == AbsenceStatus.Applied
                     && a.DeletedAt == null
                     && a.Date >= monthStart
                     && a.Date < monthEnd)
            .OrderBy(a => a.Date)
            .ToListAsync(cancellationToken);

         // Si no ha alcanzado el umbral legal de 3 días en el mes, no se crea falta disciplinaria (los días 1 y 2 no generan falta)
         if (appliedMonthAbsences.Count < EscalationThreshold)
         {
            return null;
         }

         // 2. Obtener la empresa del colaborador para determinar el código legal correspondiente
         var collaboratorCompany = await _unitOfWork.Collaborators.Entities
            .AsNoTracking()
            .Where(c => c.Id == collaboratorId)
            .Select(c => new
            {
               c.CompanyId,
               c.Company.Code,
               c.Company.Alias
            })
            .FirstOrDefaultAsync(cancellationToken);

         if (collaboratorCompany is null || collaboratorCompany.CompanyId == Guid.Empty)
         {
            return _errorManager.ThrowBadRequest<DisciplinaryFault?>(
               "No se encontró la empresa del colaborador para escalar la inasistencia.",
               "ERP:NotFound");
         }

         // Resolver código de falta muy grave legal según la empresa (ALPAC: FMG-ALP-04 / AVASA: FMG-AVS-04)
         string? legalFaultCode = null;
         if (!string.IsNullOrWhiteSpace(collaboratorCompany.Code) &&
             AbsenceEscalationCodesByCompany.TryGetValue(collaboratorCompany.Code, out var codeByCode))
         {
            legalFaultCode = codeByCode;
         }
         else if (!string.IsNullOrWhiteSpace(collaboratorCompany.Alias) &&
                  AbsenceEscalationCodesByCompany.TryGetValue(collaboratorCompany.Alias, out var codeByAlias))
         {
            legalFaultCode = codeByAlias;
         }

         if (string.IsNullOrWhiteSpace(legalFaultCode))
         {
            return _errorManager.ThrowBadRequest<DisciplinaryFault?>(
               $"No hay código de falta muy grave por inasistencia de tres días configurado para la empresa '{collaboratorCompany.Alias ?? collaboratorCompany.Code}'.",
               "ERP:NotFound");
         }

         // 3. Buscar el catálogo legal de la Falta Muy Grave
         var escalationCatalog = await _unitOfWork.FaultCatalogs.Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(fc =>
               fc.CompanyId == collaboratorCompany.CompanyId
               && fc.FaultSeverity == FaultSeverity.VerySerious
               && fc.IsActive
               && fc.DeletedAt == null
               && fc.Code == legalFaultCode, cancellationToken);

         if (escalationCatalog is null)
         {
            return _errorManager.ThrowBadRequest<DisciplinaryFault?>(
               $"No se encontró en el catálogo la falta muy grave '{legalFaultCode}' para la empresa '{collaboratorCompany.Alias ?? collaboratorCompany.Code}'.",
               "ERP:NotFound");
         }

         // 4. Verificar si ya existe una Falta Muy Grave activa por inasistencias en este mes calendario
         var existingEscalatedFault = await _unitOfWork.DisciplinaryFaults.Entities
            .FirstOrDefaultAsync(f =>
               f.CollaboratorId == collaboratorId
               && f.FaultCatalogId == escalationCatalog.Id
               && f.Severity == FaultSeverity.VerySerious
               && f.DeletedAt == null
               && f.OccurrenceDate >= monthStart
               && f.OccurrenceDate < monthEnd, cancellationToken);

         if (existingEscalatedFault != null)
         {
            // Vincular las inasistencias que aún no tengan el ID asociado
            foreach (var absence in appliedMonthAbsences.Where(a => a.DisciplinaryFaultId == null))
            {
               absence.DisciplinaryFaultId = existingEscalatedFault.Id;
               await _unitOfWork.Absences.UpdateAsync(absence);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return existingEscalatedFault;
         }

         // 5. Generar código único para la Falta Muy Grave y registrarla de forma segura
         var maxOccurrenceDate = appliedMonthAbsences.Max(f => f.Date);
         DisciplinaryFault? escalatedFault = null;

         await _unitOfWork.ExecuteWithUniqueCodeRetryAsync(async token =>
         {
            var (isSuccess, faultCode) = await _codeGenerator.GenerateUniqueDisciplinaryFaultCodeAsync(
               collaboratorCompany.CompanyId,
               maxOccurrenceDate,
               token);

            if (!isSuccess || string.IsNullOrWhiteSpace(faultCode))
            {
               _errorManager.ThrowBadRequest<bool>(
                  "No se pudo generar el código único para la falta disciplinaria muy grave.",
                  "ERP:FAULT_CODE_GENERATION_FAILED");
               return;
            }

            escalatedFault = new DisciplinaryFault
            {
               CollaboratorId = collaboratorId,
               Code = faultCode,
               FaultCatalogId = escalationCatalog.Id,
               OccurrenceDate = maxOccurrenceDate,
               Severity = FaultSeverity.VerySerious,
               Source = FaultSource.SystemEscalation,
               Status = FaultStatus.Open,
               RegisteredBy = userId,
               ParentFaultId = null,
               Evidence = "{}"
            };

            await _unitOfWork.DisciplinaryFaults.RegisterDisciplinaryFault(escalatedFault);
            await _unitOfWork.SaveChangesAsync(token);

            // 6. Vincular todas las inasistencias del mes a esa Falta Muy Grave
            foreach (var absence in appliedMonthAbsences)
            {
               absence.DisciplinaryFaultId = escalatedFault.Id;
               await _unitOfWork.Absences.UpdateAsync(absence);
            }

            await _unitOfWork.SaveChangesAsync(token);
         }, cancellationToken);

         return escalatedFault;
      }

      /// <summary>
      /// Escalamiento ordinario de faltas disciplinarias por reincidencia general:
      /// 3 faltas leves -> 1 falta grave (FG-*-01).
      /// 3 faltas graves -> 1 falta muy grave (FMG-*-01).
      /// Excluye faltas vinculadas a inasistencias, las cuales tienen su propio flujo legal.
      /// </summary>
      public async Task<bool> ExecuteEscalationProcess(DisciplinaryFault targetFault, CancellationToken cancellationToken)
      {
         // Si la falta es muy grave no escala
         if (targetFault.Severity == FaultSeverity.VerySerious) return false;

         var nextSeverity = targetFault.Severity switch
         {
            FaultSeverity.Minor => FaultSeverity.Serious,
            FaultSeverity.Serious => FaultSeverity.VerySerious,
            _ => (FaultSeverity?)null
         };

         if (nextSeverity is null) return false;

         var (monthStart, monthEnd) = GetMonthBoundaries(targetFault.OccurrenceDate);

         // Solo faltas ordinarias aún no absorbidas (excluyendo faltas de inasistencias)
         var eligibleFaults = await _unitOfWork.DisciplinaryFaults.Entities
            .Where(f =>
               f.DeletedAt == null
               && f.CollaboratorId == targetFault.CollaboratorId
               && f.Severity == targetFault.Severity
               && f.ParentFaultId == null
               && f.OccurrenceDate >= monthStart
               && f.OccurrenceDate < monthEnd
               && !f.Absences.Any(a => a.DeletedAt == null))
            .OrderBy(f => f.OccurrenceDate)
            .ThenBy(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

         if (eligibleFaults.Count < EscalationThreshold) return false;

         var faultsToLink = eligibleFaults.Take(EscalationThreshold).ToList();

         var company = await _unitOfWork.Collaborators.Entities
            .AsNoTracking()
            .Where(c => c.Id == targetFault.CollaboratorId)
            .Select(c => new
            {
               c.CompanyId,
               c.Company.Code,
               c.Company.Alias
            })
            .FirstOrDefaultAsync(cancellationToken);

         if (company is null || company.CompanyId == Guid.Empty)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "No se encontró la empresa del colaborador para escalar la falta.",
               "ERP:NotFound");
         }

         if (string.IsNullOrWhiteSpace(company.Code)
            || !RepeatFaultCodesByCompany.TryGetValue((company.Code, nextSeverity.Value), out var repeatFaultCode))
         {
            return _errorManager.ThrowBadRequest<bool>(
               $"No hay código de falta por reincidencia configurado para la empresa '{company.Code}'.",
               "ERP:NotFound");
         }

         var escalationCatalog = await _unitOfWork.FaultCatalogs.Entities
            .AsNoTracking()
            .Where(fc =>
               fc.CompanyId == company.CompanyId
               && fc.FaultSeverity == nextSeverity
               && fc.IsActive
               && fc.DeletedAt == null
               && fc.Code == repeatFaultCode)
            .OrderBy(fc => fc.Code)
            .FirstOrDefaultAsync(cancellationToken);

         if (escalationCatalog is null)
         {
            return _errorManager.ThrowBadRequest<bool>(
               $"No hay código de falta por reincidencia configurado para '{company.Alias}' / '{nextSeverity}'.",
               "ERP:NotFound");
         }

         var occurrenceDate = faultsToLink.Max(f => f.OccurrenceDate);
         DisciplinaryFault? escalatedFault = null;

         await _unitOfWork.ExecuteWithUniqueCodeRetryAsync(async token =>
         {
            var (IsSuccess, faultCode) = await _codeGenerator.GenerateUniqueDisciplinaryFaultCodeAsync(
               company.CompanyId,
               occurrenceDate,
               token);

            if (!IsSuccess || string.IsNullOrWhiteSpace(faultCode))
            {
               _errorManager.ThrowBadRequest<bool>(
                  "No se pudo generar el código falta disciplinaria.",
                  "ERP:FAULT_CODE_GENERATION_FAILED");
               return;
            }

            escalatedFault = new DisciplinaryFault
            {
               CollaboratorId = targetFault.CollaboratorId,
               Code = faultCode,
               FaultCatalogId = escalationCatalog.Id,
               OccurrenceDate = occurrenceDate,
               Severity = nextSeverity.Value,
               Source = FaultSource.SystemEscalation,
               Status = FaultStatus.Open,
               RegisteredBy = targetFault.RegisteredBy,
               ParentFaultId = null,
               Evidence = "{}"
            };

            await _unitOfWork.DisciplinaryFaults.RegisterDisciplinaryFault(escalatedFault);
            await _unitOfWork.SaveChangesAsync(token);

            foreach (var fault in faultsToLink)
            {
               fault.ParentFaultId = escalatedFault.Id;
               await _unitOfWork.DisciplinaryFaults.UpdateAsync(fault);
            }

            await _unitOfWork.SaveChangesAsync(token);
         }, cancellationToken);

         // Reevalúa la falta padre por si completa el umbral del siguiente nivel (p. ej. grave → muy grave)
         if (escalatedFault is not null)
            await ExecuteEscalationProcess(escalatedFault, cancellationToken);

         return true;
      }

      public async Task CancelLinkedFaults(DisciplinaryFault parentFault, CancellationToken cancellationToken)
      {
         var childFaults = await _unitOfWork.DisciplinaryFaults.Entities
            .Where(cf =>
               cf.ParentFaultId == parentFault.Id
               && cf.DeletedAt == null
               && cf.Status != FaultStatus.SanctionApplied
               && cf.Status != FaultStatus.Prescribed
               && cf.Status != FaultStatus.Cancelled)
            .ToListAsync(cancellationToken);

         foreach (var childFault in childFaults)
         {
            childFault.DeletedAt = DateTime.UtcNow;
            childFault.Status = FaultStatus.Cancelled;
            childFault.CancellationReason = parentFault.CancellationReason;
            await _unitOfWork.DisciplinaryFaults.UpdateAsync(childFault);
         }
      }

      public async Task MarkLinkedFaultsAsSanctionApplied(
         DisciplinaryFault parentFault,
         CancellationToken cancellationToken)
      {
         var childFaults = await _unitOfWork.DisciplinaryFaults.Entities
            .Where(cf =>
               cf.ParentFaultId == parentFault.Id
               && cf.DeletedAt == null
               && cf.Status != FaultStatus.SanctionApplied
               && cf.Status != FaultStatus.Prescribed
               && cf.Status != FaultStatus.Cancelled)
            .ToListAsync(cancellationToken);

         foreach (var childFault in childFaults)
         {
            childFault.Status = FaultStatus.SanctionApplied;
            await _unitOfWork.DisciplinaryFaults.UpdateAsync(childFault);
            // Propaga a nietas (p. ej. leves bajo una grave absorbida)
            await MarkLinkedFaultsAsSanctionApplied(childFault, cancellationToken);
         }
      }

      public (DateOnly Start, DateOnly End) GetMonthBoundaries(DateOnly occurrenceDate)
      {
         var monthStart = new DateOnly(occurrenceDate.Year, occurrenceDate.Month, 1);
         var monthEnd = monthStart.AddMonths(1);

         return (monthStart, monthEnd);
      }
   }
}
