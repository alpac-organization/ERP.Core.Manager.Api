using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Absences.v1.Handlers
{
    public class DeleteAbsenceHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager)
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

            // Soft delete de la inasistencia
            absence.DeletedAt = DateTime.Now;

            // Si tenía una falta disciplinaria asociada, también se anula lógicamente
            if (absence.DisciplinaryFault != null)
            {
                absence.DisciplinaryFault.DeletedAt = DateTime.Now;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
