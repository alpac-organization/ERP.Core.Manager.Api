using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Handlers
{
    public class ProcessVacationRequestHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager): AlpacBaseHandler<ProcessPermitApplicationCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(ProcessPermitApplicationCommand request, CancellationToken cancellationToken)
        {

            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse; 
            }

            var vacationRequest = await _unitOfWork.PermitApplications.Entities
                .Where(vr => vr.Id == request.VacationRequestId)
                .FirstOrDefaultAsync(cancellationToken);

            if (vacationRequest is null)
            {
                return _errorManager.ThrowBadRequest<bool>("No se encontro la solicitud de vacaciones", "ERP:001");
            }

            var user = await _unitOfWork.Users.Entities
                .Where(u => u.Id == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return _errorManager.ThrowBadRequest<bool>("No se encontro el usuario asociado a la solicitud", "ERP:001");
            }

            var collaboratorInformation = await _unitOfWork.Collaborators.Entities
                .Where(c => c.CompanyId == request.CompanyId)
                .Where(c => c.Id == vacationRequest.CollaboratorId)
                .Where(c => c.IdentificationNumber == user.IdentificationNumber)
                .AnyAsync(cancellationToken);



            //Información de vacaciones para reducción de información
            var vacationInformation = await _unitOfWork.Vacations.Entities
                .Where(v => v.CollaboratorId == vacationRequest.CollaboratorId)
                .FirstOrDefaultAsync(cancellationToken);

            if (vacationInformation is null)
            {
                return _errorManager.ThrowBadRequest<bool>("No se encontro registro de vacaciones del colaborador que tienes presente la solicitud", "ERP:01");
            }

            switch (vacationRequest.Type)
            {
                case PermitApplicationType.DonatedVacations:
                {

                    
                    

                    //Validar las vacaciones, descontar las vacaciones
                    break;
                }
                case PermitApplicationType.MedicalAppointment:
                {



                    

                    break;   
                }
                default:
                {
                    return _errorManager.ThrowBadRequest<bool>("Este tipo de solicitud no se encuentra disponible", "ERP:001");
                }
            }

            // if (vacationInformation is null)
            // {
            //     return _errorManager.ThrowBadRequest<bool>("No se encontro registro de información de vacaciones", "ERP:001");
            // }

            // //Descontamos los días de vacaciones si la solicitud es aprobada, de lo contrario no se descuenta
            // var amountDays = (vacationRequest.EndDate - vacationRequest.StartDate)?.Days ?? + 1;

            // await _unitOfWork.Vacations.UpdateAsync(new ()
            // {
            //     Id = vacationInformation.Id,
            //     CollaboratorId = vacationRequest.CollaboratorId,
            //     AvailableVacations = request.IsApproved ? vacationInformation.AvailableVacations - amountDays : vacationInformation.AvailableVacations,
            //     EnjoyedVacation = request.IsApproved ? vacationInformation.EnjoyedVacation + amountDays : vacationInformation.EnjoyedVacation,
            // });

            // if (!collaboratorAssociated && (access.Role!.RoleType == RoleType.Administrator || access.Role.RoleType == RoleType.Manager))
            // {
            //     //Actualizamos el estado de la solicitud de vacaciones
            //     await _unitOfWork.PermitApplications.UpdateAsync(new ()
            //     {
            //         Id =          request.VacationRequestId,
            //         Status =      request.IsApproved ? PermitApplicationStatus.Approved : PermitApplicationStatus.Rejected,
            //         ApprovedBy =  request.IsApproved ? user.Fullname : null,
            //         RejectedBy = !request.IsApproved ? user.Fullname : null,
            //     });  

            //     await _unitOfWork.SaveChangesAsync(cancellationToken);

            // }
            // else if(collaboratorAssociated && access.Role!.RoleType == RoleType.Administrator)
            // {
            //     await _unitOfWork.PermitApplications.UpdateAsync(new ()
            //     {
            //         Id =          request.VacationRequestId,    
            //         Status =      request.IsApproved ? PermitApplicationStatus.Approved : PermitApplicationStatus.Rejected,
            //         ApprovedBy =  request.IsApproved ? user.Fullname : null,
            //         RejectedBy = !request.IsApproved ? user.Fullname : null,
            //     });

            //     await _unitOfWork.SaveChangesAsync(cancellationToken);
            // }
            // else if(collaboratorAssociated && access.Role!.RoleType == RoleType.Manager)
            // {
            //     return _errorManager.ThrowBadRequest<bool>("No tienes permisos para procesar esta solicitud, una persona de administración debe revisarla", "ERP:003");
            // }

            return true;
        }
    }
}