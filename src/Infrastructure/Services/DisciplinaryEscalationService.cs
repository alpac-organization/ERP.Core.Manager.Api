using System.Collections.Frozen;
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

         // Solo faltas aún no absorbidas por una escalada previa
         var eligibleFaults = await _unitOfWork.DisciplinaryFaults.Entities
            .Where(f =>
               f.DeletedAt == null
               && f.CollaboratorId == targetFault.CollaboratorId
               && f.Severity == targetFault.Severity
               && f.ParentFaultId == null
               && f.OccurrenceDate >= monthStart
               && f.OccurrenceDate < monthEnd)
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
