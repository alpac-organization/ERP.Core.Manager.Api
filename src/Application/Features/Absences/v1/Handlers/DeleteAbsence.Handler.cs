using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Absences.v1.Handlers
{
    public class DeleteAbsenceHandler(
        IUnitOfWork _unitOfWork, 
        IErrorManager _errorManager, 
        IDisciplinaryEscalation _disciplinaryEscalation)
        : AlpacBaseHandler<DeleteAbsenceCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(DeleteAbsenceCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);
            if (!access.IsSuccess)
            {
                return access.ErrorResponse;
            }

            var absence = await _unitOfWork.Absences.Entities
                .Include(a => a.Collaborator)
                .Include(a => a.Payroll)
                .Include(a => a.DisciplinaryFault)
                    .ThenInclude(df => df!.Sanctions)
                .FirstOrDefaultAsync(a => a.Id == request.AbsenceId 
                                       && a.Collaborator.CompanyId == request.CompanyId
                                       && a.DeletedAt == null, cancellationToken);

            if (absence is null)
            {
                return _errorManager.ThrowNotFound<bool>("No se encontró el registro de inasistencia especificado o ya fue eliminado.", "ERP:004");
            }

            // Validar que no pertenezca a un ciclo de nómina cerrado
            if (absence.Payroll != null && (absence.Payroll.Status == PayrollStatus.Closed || absence.Payroll.Status == PayrollStatus.Completed))
            {
                return _errorManager.ThrowBadRequest<bool>("No se puede eliminar una inasistencia asociada a un ciclo de nómina cerrado.", "ERP:006");
            }

            // Si la inasistencia estaba vinculada a una Falta Muy Grave por acumulación de 3 inasistencias en el mes
            if (absence.DisciplinaryFault != null)
            {
                var escalatedFault = absence.DisciplinaryFault;

                // 1. Validar que la falta muy grave no tenga sanción aplicada
                if (escalatedFault.Status == FaultStatus.SanctionApplied ||
                    escalatedFault.Sanctions.Any(s => s.DeletedAt == null))
                {
                    return _errorManager.ThrowBadRequest<bool>(
                        "No se puede anular la inasistencia porque la falta muy grave por acumulación de 3 inasistencias ya cuenta con una sanción aplicada en Recursos Humanos.",
                        "ERP:007");
                }

                // 2. Contar cuántas inasistencias aplicadas quedarán en el mes tras anular esta
                var (monthStart, monthEnd) = _disciplinaryEscalation.GetMonthBoundaries(absence.Date);
                var remainingAppliedCount = await _unitOfWork.Absences.Entities
                    .CountAsync(a => a.CollaboratorId == absence.CollaboratorId
                                  && a.Id != absence.Id
                                  && a.Status == AbsenceStatus.Applied
                                  && a.DeletedAt == null
                                  && a.Date >= monthStart
                                  && a.Date < monthEnd, cancellationToken);

                // Si al anular esta inasistencia quedan menos del umbral legal (3 días en el mes), se anula la falta muy grave
                if (remainingAppliedCount < 3)
                {
                    escalatedFault.DeletedAt = DateTime.Now;
                    await _unitOfWork.DisciplinaryFaults.UpdateAsync(escalatedFault);

                    // Desvincular las inasistencias hermanas de ese mes para que queden sin falta asociada
                    var siblingAbsences = await _unitOfWork.Absences.Entities
                        .Where(a => a.DisciplinaryFaultId == escalatedFault.Id && a.Id != absence.Id && a.DeletedAt == null)
                        .ToListAsync(cancellationToken);

                    foreach (var sibling in siblingAbsences)
                    {
                        sibling.DisciplinaryFaultId = null;
                        await _unitOfWork.Absences.UpdateAsync(sibling);
                    }
                }

                absence.DisciplinaryFaultId = null;
            }

            // Traspaso automático del Séptimo Día si esta inasistencia lo tenía asignado
            if (absence.SeventhDayDeduction > 0)
            {
                int diff = (7 + (absence.Date.DayOfWeek - DayOfWeek.Monday)) % 7;
                var weekStart = absence.Date.AddDays(-diff);
                var weekEnd = weekStart.AddDays(6);

                var siblingAbsenceInWeek = await _unitOfWork.Absences.Entities
                    .Where(a => a.CollaboratorId == absence.CollaboratorId
                             && a.Id != absence.Id
                             && a.Status == AbsenceStatus.Applied
                             && a.DeletedAt == null
                             && a.Date >= weekStart
                             && a.Date <= weekEnd
                             && a.SeventhDayDeduction == 0)
                    .OrderBy(a => a.Date)
                    .FirstOrDefaultAsync(cancellationToken);

                if (siblingAbsenceInWeek != null)
                {
                    siblingAbsenceInWeek.SeventhDayDeduction = absence.SeventhDayDeduction;
                    await _unitOfWork.Absences.UpdateAsync(siblingAbsenceInWeek);
                }
            }

            // Revertir deducciones monetarias
            absence.SalaryDeduction = 0;
            absence.SeventhDayDeduction = 0;
            absence.TravelExpensesDeduction = 0;

            // Cambiar estado a Cancelled y marcar soft delete (Defensa en profundidad)
            absence.Status = AbsenceStatus.Cancelled;
            absence.DeletedAt = DateTime.Now;

            await _unitOfWork.Absences.UpdateAsync(absence);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
