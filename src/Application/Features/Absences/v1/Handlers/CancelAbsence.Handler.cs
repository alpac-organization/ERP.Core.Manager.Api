using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Absences.v1.Handlers
{
    public class CancelAbsenceHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager)
        : AlpacBaseHandler<CancelAbsenceCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(CancelAbsenceCommand request, CancellationToken cancellationToken)
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
                .FirstOrDefaultAsync(a => a.Id == request.AbsenceId 
                                       && a.Collaborator.CompanyId == request.CompanyId
                                       && a.DeletedAt == null, cancellationToken);

            if (absence is null)
            {
                return _errorManager.ThrowNotFound<bool>("No se encontró el registro de inasistencia especificado.", "ERP:004");
            }

            if (absence.Status == AbsenceStatus.Cancelled)
            {
                return _errorManager.ThrowBadRequest<bool>("La inasistencia ya se encuentra cancelada.", "ERP:005");
            }

            // Validar que no pertenezca a un ciclo de nómina ya cerrado o completado
            if (absence.Payroll != null && (absence.Payroll.Status == PayrollStatus.Closed || absence.Payroll.Status == PayrollStatus.Completed))
            {
                return _errorManager.ThrowBadRequest<bool>("No se puede cancelar una inasistencia que ya fue liquidada en un ciclo de nómina cerrado.", "ERP:006");
            }

            // Cambiar estado a Cancelled y registrar auditoría de RRHH
            absence.Status = AbsenceStatus.Cancelled;
            absence.ReviewedBy = request.UserId;
            absence.ReviewedAt = DateTime.Now;
            absence.CancellationReason = request.CancellationReason;

            // Revertir deducciones monetarias
            absence.SalaryDeduction = 0;
            absence.SeventhDayDeduction = 0;
            absence.TravelExpensesDeduction = 0;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
