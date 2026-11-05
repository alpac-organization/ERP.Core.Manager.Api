using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Commands;

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

            // 1. Obtener salario activo y calcular deducción de salario ordinario
            var activeSalary = absence.Collaborator.Salaries
                .Where(s => s.EndDate == null)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefault();

            if (activeSalary != null && activeSalary.AmountSalary > 0)
            {
                var dailySalary = activeSalary.AmountSalary / 30m;
                absence.SalaryDeduction = Math.Round(dailySalary, 2);

                // Regla del Séptimo Día (Criterio A: Tope Semanal de 1 Séptimo Día en la semana Lunes a Domingo)
                int diff = (7 + (absence.Date.DayOfWeek - DayOfWeek.Monday)) % 7;
                var weekStart = absence.Date.AddDays(-diff);
                var weekEnd = weekStart.AddDays(6);

                var alreadyDeductedSeventhDay = await _unitOfWork.Absences.Entities
                    .AnyAsync(a => a.CollaboratorId == absence.CollaboratorId
                                && a.Id != absence.Id
                                && a.Status == AbsenceStatus.Applied
                                && a.DeletedAt == null
                                && a.Date >= weekStart
                                && a.Date <= weekEnd
                                && a.SeventhDayDeduction > 0, cancellationToken);

                if (alreadyDeductedSeventhDay)
                {
                    // Tope alcanzado: el descanso semanal ya fue deducido por otra inasistencia en la misma semana
                    absence.SeventhDayDeduction = 0.0m;
                }
                else
                {
                    // Primera inasistencia en la semana: deduce el séptimo día completo
                    absence.SeventhDayDeduction = Math.Round(dailySalary, 2);
                }
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

            await _unitOfWork.Absences.UpdateAsync(absence);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 4. Verificar acumulación de 3 inasistencias en el mes calendario (continuas o discontinuas)
            // Si el colaborador acumula 3 o más inasistencias en el mes calendario, se genera directamente
            // la Falta Muy Grave (FMG-ALP-04 / FMG-AVS-04). No se crean faltas leves intermedias.
            var escalatedFault = await _disciplinaryEscalation.CheckAndEscalateAbsencesAsync(
                absence.CollaboratorId,
                absence.Date,
                request.UserId,
                cancellationToken);

            if (escalatedFault != null)
            {
                absence.DisciplinaryFaultId = escalatedFault.Id;
                await _unitOfWork.Absences.UpdateAsync(absence);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
    }
}
