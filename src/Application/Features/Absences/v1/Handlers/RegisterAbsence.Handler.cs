using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Mappings;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Absences.v1.Handlers
{
    public class RegisterAbsenceHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager)
        : AlpacBaseHandler<RegisterAbsenceCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(RegisterAbsenceCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);
            if (!access.IsSuccess)
            {
                return access.ErrorResponse;
            }

            var collaborator = await _unitOfWork.Collaborators.Entities
                .Include(c => c.WorkingInformation)
                .FirstOrDefaultAsync(c => c.Id == request.CollaboratorId && c.CompanyId == request.CompanyId, cancellationToken);

            if (collaborator is null)
            {
                return _errorManager.ThrowNotFound<bool>("No se encontró el colaborador especificado en esta empresa.", "ERP:004");
            }

            // 1. Verificar si ya existe una inasistencia activa/pendiente registrada en la misma fecha (no eliminada lógicamente)
            var existingAbsence = await _unitOfWork.Absences.Entities
                .AnyAsync(a => a.CollaboratorId == request.CollaboratorId 
                            && a.Date == request.Date 
                            && a.Status != AbsenceStatus.Cancelled
                            && a.DeletedAt == null, cancellationToken);

            if (existingAbsence)
            {
                return _errorManager.ThrowBadRequest<bool>("Ya existe un registro de inasistencia para este colaborador en la fecha especificada.", "ERP:005");
            }

            // 2. Verificar si cuenta con un permiso aprobado en esa fecha
            var hasApprovedPermit = await _unitOfWork.PermitApplications.Entities
                .AnyAsync(p => p.CollaboratorId == request.CollaboratorId
                            && p.Status == PermitApplicationStatus.Approved
                            && p.StartDate <= request.Date
                            && p.EndDate >= request.Date, cancellationToken);

            if (hasApprovedPermit)
            {
                return _errorManager.ThrowBadRequest<bool>("El colaborador cuenta con un permiso aprobado en la fecha indicada.", "ERP:006");
            }

            // 3. Crear inasistencia en estado Pending
            var absence = request.ToAbsenceEntity();

            await _unitOfWork.Absences.RegisterAbsence(absence);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
