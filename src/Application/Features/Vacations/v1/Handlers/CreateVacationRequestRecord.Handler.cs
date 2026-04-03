using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Handlers
{
    public class CreateVacationRequestRecordHandler(IUnitOfWork _unitOfWork,IErrorManager _errorManager) : AlpacBaseHandler<CreateVacationRequestRecordCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(CreateVacationRequestRecordCommand request, CancellationToken cancellationToken)
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

                var AmountDays = (int)(request.EndDate - request.StartDate).TotalDays + 1;

                if (vacationControl.AvailableVacations < AmountDays)
                {
                    return _errorManager.ThrowBadRequest<bool>(
                        $"El colaborador no tiene suficientes vacaciones disponibles para solicitar {AmountDays} días.",
                        "ERP:005"
                    );
                }

                var overlappingRequests = await _unitOfWork.VacationRequests.Entities
                    .AnyAsync(vr => 
                        vr.CollaboratorId == collaborator.Id &&
                        vr.Status != VacationRequestStatus.Rejected  && 
                        vr.Status != VacationRequestStatus.Cancelled &&
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

                var VacationRequestEntity = new VacationRequest()
                {
                    CollaboratorId = collaborator.Id,
                    ApprovedBy = null,
                    RejectedBy = null,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    RequestedBy = $"{collaborator.FirstName.ToCapitalize()} {collaborator.SecondName?.ToCapitalize() ?? string.Empty} {collaborator.FirstLastname.ToCapitalize()} {collaborator.SecondLastname?.ToCapitalize() ?? string.Empty}".Trim(),
                    Description = request.Description,
                    CollaboratorCode = collaborator.CollaboratorCode,
                    Status = VacationRequestStatus.Pending,
                    AmountDays = AmountDays,
                };

                await _unitOfWork.VacationRequests.CreateVacationRequest(VacationRequestEntity);

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