using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Handlers
{
    public class CreatePermitApplicationHandler(IUnitOfWork _unitOfWork,IErrorManager _errorManager) : AlpacBaseHandler<CreatePermitApplicationCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(CreatePermitApplicationCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if(!access.IsSuccess) 
            {
                return access.ErrorResponse; 
            }

            if (access.Role!.RoleType == RoleType.Administrator || access.Role!.RoleType == RoleType.Operator || access.Role!.RoleType == RoleType.Manager)
            {

                var collaborator = await _unitOfWork.Collaborators.Entities
                    .FirstOrDefaultAsync(c => c.IdentificationNumber == request.IdentificationNumber && c.CompanyId == request.CompanyId, cancellationToken);

                if (collaborator is null)
                {
                    return _errorManager.ThrowBadRequest<bool>(
                        $"No se encontró un colaborador con el número de identificación {request.IdentificationNumber} en esta empresa.", 
                        "ERP:003"
                    );
                }

                DateTime finalEndDate = request.EndDate ?? request.StartDate;
                int totalDays = (int)(finalEndDate.Date - request.StartDate.Date).TotalDays + 1;

                if (request.PermitApplicationType == PermitApplicationType.Vacation)
                {
                    var vacationControl = await _unitOfWork.Vacations.Entities
                        .Where(v => v.CollaboratorId == collaborator.Id)
                        .Include(v => v.Collaborator)
                            .Where(v => v.Collaborator.IdentificationNumber == request.IdentificationNumber && v.Collaborator.CompanyId == request.CompanyId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if(vacationControl is null)
                    {
                        return _errorManager.ThrowBadRequest<bool>(
                            $"No se encontró un control de vacaciones para el colaborador con número de identificación {request.IdentificationNumber} en esta empresa.", 
                            "ERP:004"
                        );
                    }

                    if (vacationControl.AvailableVacations < totalDays)
                    {
                        return _errorManager.ThrowBadRequest<bool>(
                            $"El colaborador no tiene suficientes vacaciones disponibles para solicitar {totalDays} días.",
                            "ERP:005"
                        );
                    }
                }

                var overlappingRequests = await _unitOfWork.PermitApplications.Entities
                    .AnyAsync(vr => 
                        vr.CollaboratorId == collaborator.Id &&
                        vr.Status != PermitApplicationStatus.Rejected  && 
                        vr.Status != PermitApplicationStatus.Cancelled &&
                        request.StartDate <= vr.EndDate && 
                        request.EndDate >= vr.StartDate, 
                        cancellationToken
                    );

                if (overlappingRequests)
                {
                    return _errorManager.ThrowBadRequest<bool>(
                        $"El colaborador ya tiene una solicitud de vacaciones que se superpone con las fechas proporcionadas.",
                        "ERP:006"
                    );
                }

                var PermitApplicationEntity = new Domain.Entities.Payroll.PermitApplication()
                {
                    CollaboratorId = collaborator.Id,
                    ApprovedBy = null,
                    RejectedBy = null,
                    StartDate = request.StartDate,
                    Type = request.PermitApplicationType,
                    EndDate = finalEndDate,
                    RequestedBy = $"{collaborator.FirstName.ToCapitalize()} {collaborator.SecondName?.ToCapitalize() ?? string.Empty} {collaborator.FirstLastname.ToCapitalize()} {collaborator.SecondLastname?.ToCapitalize() ?? string.Empty}".Trim(),
                    Description = request.Description,
                    CollaboratorCode = collaborator.CollaboratorCode,
                    Status = PermitApplicationStatus.Pending,
                    AmountDays = totalDays,
                    EndTime = request.EndTime,
                    StartTime = request.StartTime
                };

                await _unitOfWork.PermitApplications.CreateVacationRequest(PermitApplicationEntity);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                return _errorManager.ThrowBadRequest<bool>("No tienes permisos para crear registros de solicitudes de vacaciones.", "ERP:002");   
            }

            return true;
        }
    }
}