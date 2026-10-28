using System.Collections.Frozen;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
   public class DisciplinaryEscalationService(IUnitOfWork _unitOfWork, IErrorManager _errorManager) : IDisciplinaryEscalation
   {
      private const int EscalationThreshold = 3;

      private static readonly FrozenDictionary<(string Alias, FaultSeverity Severity), string> RepeatFaultCodesByCompany =
         new Dictionary<(string, FaultSeverity), string>
         {
            [("ALPAC", FaultSeverity.Serious)] = "FG-ALP-01",
            [("ALPAC", FaultSeverity.VerySerious)] = "FMG-ALP-01",
            [("AVASA", FaultSeverity.Serious)] = "FG-AVS-01",
            [("AVASA", FaultSeverity.VerySerious)] = "FMG-AVS-01",
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
               c.Company.Alias
            })
            .FirstOrDefaultAsync(cancellationToken);

         if (company is null || company.CompanyId == Guid.Empty)
         {
            return _errorManager.ThrowBadRequest<bool>(
               "No se encontró la empresa del colaborador para escalar la falta.",
               "ERP:NotFound");
         }

         if (string.IsNullOrWhiteSpace(company.Alias)
            || !RepeatFaultCodesByCompany.TryGetValue((company.Alias, nextSeverity.Value), out var repeatFaultCode))
         {
            return _errorManager.ThrowBadRequest<bool>(
               $"No hay código de falta por reincidencia configurado para la empresa '{company.Alias}'.",
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

         var escalatedFault = new DisciplinaryFault
         {
            CollaboratorId = targetFault.CollaboratorId,
            FaultCatalogId = escalationCatalog.Id,
            OccurrenceDate = faultsToLink.Max(f => f.OccurrenceDate),
            Severity = nextSeverity.Value,
            Source = FaultSource.SystemEscalation,
            Status = FaultStatus.Open,
            RegisteredBy = targetFault.RegisteredBy,
            ParentFaultId = null,
            Evidence = "{}"
         };

         await _unitOfWork.DisciplinaryFaults.RegisterDisciplinaryFault(escalatedFault);
         await _unitOfWork.SaveChangesAsync(cancellationToken);

         foreach (var fault in faultsToLink)
         {
            fault.ParentFaultId = escalatedFault.Id;
            await _unitOfWork.DisciplinaryFaults.UpdateAsync(fault);
         }

         await _unitOfWork.SaveChangesAsync(cancellationToken);

         return true;
      }

      public (DateOnly Start, DateOnly End) GetMonthBoundaries(DateOnly occurrenceDate)
      {
         var monthStart = new DateOnly(occurrenceDate.Year, occurrenceDate.Month, 1);
         var monthEnd = monthStart.AddMonths(1);

         return (monthStart, monthEnd);
      }
   }
}
