using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Manager.Api.Application.Commons.Mappings;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Absences.v1.Handlers
{
    public class RegisterAbsenceHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager)
        : BaseValidatorHandler<RegisterAbsenceCommand, bool>(_unitOfWork, _errorManager)
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

            // 1. Validar días inhábiles o no laborables según ley y jornada del colaborador
            if (request.Date.DayOfWeek == DayOfWeek.Sunday)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    "No se puede registrar una inasistencia en día domingo ya que es el día de descanso semanal obligatorio.",
                    "ERP:005");
            }

            if (request.Date.DayOfWeek == DayOfWeek.Saturday && !collaborator.DoesWorkSaturdays)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    "El colaborador no labora los días sábado según su jornada asignada (Lunes a Viernes).",
                    "ERP:005");
            }

            // 2. Validar si la fecha coincide con un día feriado oficial (nacional o de su sucursal)
            var branchId = collaborator.WorkingInformation?.BranchId;
            var holidayName = await _unitOfWork.Holidays.Entities
                .Where(h => h.IsActive 
                         && h.DeletedAt == null 
                         && h.Day == request.Date.Day 
                         && h.Month == request.Date.Month 
                         && (h.IsGlobal || (branchId != null && h.BranchId == branchId)))
                .Select(h => h.HolidayName)
                .FirstOrDefaultAsync(cancellationToken);

            if (holidayName != null)
            {
                var label = string.IsNullOrWhiteSpace(holidayName) ? "Feriado oficial" : holidayName;
                return _errorManager.ThrowBadRequest<bool>(
                    $"No se puede registrar una inasistencia en día feriado ('{label}'), ya que es un día de descanso remunerado obligatorio.",
                    "ERP:005");
            }

            // 3. Verificar si ya existe una inasistencia activa/pendiente registrada en la misma fecha (no eliminada lógicamente)
            var existingAbsence = await _unitOfWork.Absences.Entities
                .AnyAsync(a => a.CollaboratorId == request.CollaboratorId 
                            && a.Date == request.Date 
                            && a.Status != AbsenceStatus.Cancelled
                            && a.DeletedAt == null, cancellationToken);

            if (existingAbsence)
            {
                return _errorManager.ThrowBadRequest<bool>("Ya existe un registro de inasistencia para este colaborador en la fecha especificada.", "ERP:005");
            }

            // 4. Verificar si cuenta con un permiso aprobado en esa fecha
            var hasApprovedPermit = await _unitOfWork.PermitApplications.Entities
                .AnyAsync(p => p.CollaboratorId == request.CollaboratorId
                            && p.DeletedAt == null
                            && p.Status == PermitApplicationStatus.Approved
                            && p.StartDate <= request.Date
                            && p.EndDate >= request.Date, cancellationToken);

            if (hasApprovedPermit)
            {
                return _errorManager.ThrowBadRequest<bool>("El colaborador cuenta con un permiso aprobado en la fecha indicada.", "ERP:006");
            }

            // 5. Verificar si cuenta con un subsidio médico activo en esa fecha
            var requestDateTime = request.Date.ToDateTime(TimeOnly.MinValue);
            var hasActiveSubsidy = await _unitOfWork.Subsidies.Entities
                .AnyAsync(s => s.CollaboratorId == request.CollaboratorId
                            && s.DeletedAt == null
                            && s.StartDate <= requestDateTime
                            && s.EndDate >= requestDateTime, cancellationToken);

            if (hasActiveSubsidy)
            {
                return _errorManager.ThrowBadRequest<bool>("El colaborador cuenta con un subsidio médico activo en la fecha indicada.", "ERP:006");
            }

            // 6. Crear inasistencia en estado Pending
            var absence = request.ToAbsenceEntity();

            await _unitOfWork.Absences.RegisterAbsence(absence);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
