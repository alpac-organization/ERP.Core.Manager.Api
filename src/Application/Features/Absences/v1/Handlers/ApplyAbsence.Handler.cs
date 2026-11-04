using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Mappings;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Commands;
using ERP.Core.Database.Domain.Entities.Payrolls;

namespace ERP.Core.Manager.Api.Application.Features.Absences.v1.Handlers
{
    public class ApplyAbsenceHandler(
        IUnitOfWork _unitOfWork, 
        IErrorManager _errorManager, 
        IDisciplinaryEscalation _disciplinaryEscalation)
        : AlpacBaseHandler<ApplyAbsenceCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(ApplyAbsenceCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);
            if (!access.IsSuccess)
            {
                return access.ErrorResponse;
            }

            var absence = await _unitOfWork.Absences.Entities
                .Include(a => a.Collaborator)
                    .ThenInclude(c => c.Salaries)
                .Include(a => a.Collaborator)
                    .ThenInclude(c => c.AssignedTravelExpenses)
                .FirstOrDefaultAsync(a => a.Id == request.AbsenceId 
                                       && a.Collaborator.CompanyId == request.CompanyId 
                                       && a.DeletedAt == null, cancellationToken);

            if (absence is null)
            {
                return _errorManager.ThrowNotFound<bool>("No se encontró el registro de inasistencia especificado.", "ERP:004");
            }

            if (absence.Status == AbsenceStatus.Applied)
            {
                return _errorManager.ThrowBadRequest<bool>("Esta inasistencia ya ha sido aplicada previamente.", "ERP:005");
            }

            if (absence.Status == AbsenceStatus.Cancelled)
            {
                return _errorManager.ThrowBadRequest<bool>("No se puede aplicar una inasistencia que se encuentra cancelada.", "ERP:005");
            }

            // 1. Obtener salario activo y calcular deducción de salario ordinario y séptimo día
            var activeSalary = absence.Collaborator.Salaries
                .Where(s => s.EndDate == null)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefault();

            if (activeSalary != null && activeSalary.AmountSalary > 0)
            {
                var dailySalary = activeSalary.AmountSalary / 30m;
                absence.SalaryDeduction = Math.Round(dailySalary, 2);
                absence.SeventhDayDeduction = Math.Round(dailySalary, 2);
            }

            // 2. Calcular deducción de viáticos asignados activos
            var activeTravelExpenses = absence.Collaborator.AssignedTravelExpenses
                .Where(t => t.EndDate == null)
                .Sum(t => t.AmountInLocalCurrency);

            if (activeTravelExpenses > 0)
            {
                absence.TravelExpensesDeduction = Math.Round(activeTravelExpenses , 2);
            }

            // 3. Cambiar estado a Applied y registrar auditoría de RRHH
            absence.Status = AbsenceStatus.Applied;
            absence.ReviewedBy = request.UserId;
            absence.ReviewedAt = DateTime.Now;

            // 4. Integrar con el Módulo de Faltas Disciplinarias (Registrar Falta Leve por inasistencia)
            FaultCatalog? faultCatalog = null;

            if (request.FaultCatalogId.HasValue)
            {
                faultCatalog = await _unitOfWork.FaultCatalogs.Entities
                    .FirstOrDefaultAsync(fc => fc.Id == request.FaultCatalogId.Value 
                                            && fc.CompanyId == request.CompanyId 
                                            && fc.IsActive
                                            && fc.DeletedAt == null, cancellationToken);
            }

            // Buscar en el catálogo de faltas leves de la empresa por coincidencia en el nombre ("Ausencia")
            faultCatalog ??= await _unitOfWork.FaultCatalogs.Entities
                .FirstOrDefaultAsync(fc => fc.CompanyId == request.CompanyId 
                                        && fc.IsActive 
                                        && fc.DeletedAt == null
                                        && fc.FaultSeverity == FaultSeverity.Minor
                                        && (fc.Name.Contains("Ausencia") || fc.Name.Contains("ausencia")), cancellationToken);

            if (faultCatalog is null)
            {
                return _errorManager.ThrowNotFound<bool>("No se encontró en el catálogo de faltas la falta disciplinaria leve por ausencia injustificada para esta empresa.", "ERP:004");
            }

            var disciplinaryFault = absence.ToDisciplinaryFaultEntity(
                faultCatalog.Id,
                FaultSeverity.Minor,
                request.UserId);

            await _unitOfWork.DisciplinaryFaults.RegisterDisciplinaryFault(disciplinaryFault);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            absence.DisciplinaryFaultId = disciplinaryFault.Id;

            // Ejecutar el proceso de escalamiento existente (3 faltas leves en el mes = 1 falta grave automática)
            await _disciplinaryEscalation.ExecuteEscalationProcess(disciplinaryFault, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
