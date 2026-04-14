using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using System.Text.Json;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Handlers
{
    public class ProcessPermitApplicationtHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager): AlpacBaseHandler<ProcessPermitApplicationCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(ProcessPermitApplicationCommand request, CancellationToken cancellationToken)
        {

            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse; 
            }

            var permitApplication = await _unitOfWork.PermitApplications.Entities
                .Where(vr => vr.Id == request.PermitApplicationId)
                .FirstOrDefaultAsync(cancellationToken);

            if (permitApplication is null)
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
                .Where(c => c.Id == permitApplication.CollaboratorId)
                .Where(c => c.IdentificationNumber == user.IdentificationNumber)
                .AnyAsync(cancellationToken);

            //Información de vacaciones para reducción de información
            var vacationInformationSolicitante = await _unitOfWork.Vacations.Entities
                .Where(v => v.CollaboratorId == permitApplication.CollaboratorId)
                .FirstOrDefaultAsync(cancellationToken);

            if (vacationInformationSolicitante is null)
            {
                return _errorManager.ThrowBadRequest<bool>("No se encontro registro de vacaciones del solicitante", "ERP:02");
            }

            switch (permitApplication.Type)
            {
                case PermitApplicationType.DonatedVacations:
                {
                    #region Primero proceso de aprobación
                    var response = MapperInformationToApprovedFirstStep(
                        permitApplication, 
                        access.Role!.RoleType, 
                        request.IsApproved, 
                        user.Fullname ?? "unknow user"
                    );

                    if (response)
                    {
                        await _unitOfWork.PermitApplications.UpdateAsync(permitApplication);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }                            
                    else return _errorManager.ThrowBadRequest<bool>("No tienes permiso para aprobar esta solicitud", "ERP:01"); 
                    #endregion

                    #region Aprobación de solicitud del colaborador
                    if ((permitApplication.FirtsStepApproved is true || permitApplication.FirtsStepApproved is false) && request.IsApproved)
                    {
                        if (access.Role!.RoleType == RoleType.Manager && collaboratorInformation)
                        {
                            return _errorManager.ThrowBadRequest<bool>("No puedes aprobarte el proceso, no eres administrador", "ERP:03");
                        }

                        if (access.Role!.RoleType == RoleType.Administrator)
                        {
                            var vacationControlToReceive = await _unitOfWork.Vacations.Entities
                                .Include(vac  => vac.Collaborator)
                                .Where(vac  => vac.Collaborator.IdentificationNumber == permitApplication.IdentificationCollaboratorToReceive)
                                .FirstOrDefaultAsync(cancellationToken);

                            if (vacationControlToReceive is null)
                            {
                                return _errorManager.ThrowBadRequest<bool>("El colaborador que recibira las vacaciones no tiene proceso de vacaciones", "ERP:03");                            
                            }

                            //Reducimos las vacaciones para el solicitante.
                            vacationInformationSolicitante.AvailableVacations -= permitApplication.AmountDays ?? 0m;
                            vacationInformationSolicitante.EnjoyedVacation += permitApplication.AmountDays ?? 0;
                            
                            await _unitOfWork.Vacations.UpdateAsync(vacationInformationSolicitante);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);


                            //Le aumentamos la vacaciones a la persona que recibira las vacaciones.
                            vacationControlToReceive.AvailableVacations += permitApplication.AmountDays ?? 0m;
                            vacationControlToReceive.DonatedVacation += permitApplication.AmountDays ?? 0m;

                            await _unitOfWork.Vacations.UpdateAsync(vacationControlToReceive);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);

                            permitApplication.AdministratorFullName = $"{user.Fullname}";
                            permitApplication.SecondStepApproved = true;
                            permitApplication.Status = PermitApplicationStatus.Approved;

                            await _unitOfWork.PermitApplications.UpdateAsync(permitApplication);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);

                            return true;
                        }
                        else
                        {
                            return _errorManager.ThrowBadRequest<bool>("Solo administradores pueden aprobar el ultimo proceso de solicitud", "ERP:02");   
                        }
                    }
                    #endregion

                    #region Rechazar Solicitud de colaborador
                    else if((permitApplication.FirtsStepApproved is true || permitApplication.FirtsStepApproved is false) && request.IsApproved is false)
                    {

                        if (access.Role!.RoleType == RoleType.Manager && collaboratorInformation)
                        {
                            return _errorManager.ThrowBadRequest<bool>("No puedes rechazar el proceso, no eres administrador", "ERP:03");
                        }

                        if (access.Role!.RoleType == RoleType.Administrator)
                        {
                            permitApplication.AdministratorFullName = $"{user.Fullname}";
                            permitApplication.SecondStepApproved = false;
                            permitApplication.Status = PermitApplicationStatus.Rejected;

                            await _unitOfWork.PermitApplications.UpdateAsync(permitApplication);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);

                            return false;
                        }
                        else
                        {
                            return _errorManager.ThrowBadRequest<bool>("Solo administradores pueden cancelar la solicitud", "ERP:03");   
                        }
                    }
                    #endregion

                    break;
                }
                case PermitApplicationType.MedicalAppointment:
                {
                    #region Primer Proceso de aprobación
                    var response = MapperInformationToApprovedFirstStep(
                        permitApplication, 
                        access.Role!.RoleType, 
                        request.IsApproved, 
                        user.Fullname ?? "unknow user"
                    );

                    if (response)
                    {
                        await _unitOfWork.PermitApplications.UpdateAsync(permitApplication);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }                            
                    else return _errorManager.ThrowBadRequest<bool>("No tienes permiso para aprobar esta solicitud", "ERP:01");  
                    
                    #endregion

                    #region Aprobar solicitu de cita medica
                    if ((permitApplication.FirtsStepApproved is true || permitApplication.FirtsStepApproved is false) && request.IsApproved)
                    {
                        if (access.Role!.RoleType == RoleType.Manager && collaboratorInformation)
                        {
                            return _errorManager.ThrowBadRequest<bool>("No puedes aprobarte el proceso, no eres administrador", "ERP:03");
                        }

                        if (access.Role!.RoleType == RoleType.Administrator)
                        {
                            //Validar si es el dia completo que se tomo para la cita medica y deduciles lo dias de 0.5

                            var additionalInformation = JsonSerializer.Deserialize<AdditionalDataPermitApplication>(permitApplication.AdditionalData);

                            if (additionalInformation!.MedicalAppointmentData.IsFullDay)
                            {
                                vacationInformationSolicitante.AvailableVacations -= 0.5m;
                                vacationInformationSolicitante.EnjoyedVacation += 0.5m;

                                await _unitOfWork.Vacations.UpdateAsync(vacationInformationSolicitante);  
                                await _unitOfWork.SaveChangesAsync(cancellationToken);
                            }

                            permitApplication.AdministratorFullName = $"{user.Fullname}";
                            permitApplication.SecondStepApproved = true;
                            permitApplication.Status = PermitApplicationStatus.Approved;

                            await _unitOfWork.PermitApplications.UpdateAsync(permitApplication);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);

                            return true;
                        }    
                    }
                    #endregion

                    #region Rechazar solicitud del colaborador ¿
                    else if((permitApplication.FirtsStepApproved is true || permitApplication.FirtsStepApproved is false) && request.IsApproved is false)
                    {
                        if (access.Role!.RoleType == RoleType.Administrator)
                        {
                            permitApplication.AdministratorFullName = $"{user.Fullname}";
                            permitApplication.SecondStepApproved = false;
                            permitApplication.Status = PermitApplicationStatus.Rejected;

                            await _unitOfWork.PermitApplications.UpdateAsync(permitApplication);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);

                            return false;
                        }
                        else
                        {
                            return _errorManager.ThrowBadRequest<bool>("Solo administradores pueden cancelar la solicitud", "ERP:03");   
                        }
                    }     
                    #endregion           
                
                    break;
                }
                default:
                {
                    return _errorManager.ThrowBadRequest<bool>("Este tipo de solicitud no se encuentra disponible", "ERP:001");
                }
            }

            return true;
        }

        private static bool MapperInformationToApprovedFirstStep(Database.Domain.Entities.Payroll.PermitApplication permitApplication, RoleType roleType, bool isApproved, string userFullname)
        {
            if (permitApplication.FirtsStepApproved is null && isApproved is true)
            {
                if (roleType != RoleType.Supervisor || roleType != RoleType.Operator)
                {
                    permitApplication.FirtsStepApproved = true;
                    permitApplication.ManagerFullname = $"{userFullname}";

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(permitApplication.FirtsStepApproved is null && isApproved is false)
            {
                if (roleType != RoleType.Supervisor || roleType != RoleType.Operator)
                {
                    permitApplication.FirtsStepApproved = false;
                    permitApplication.ManagerFullname = $"{userFullname}";

                    return true;
                }
                else
                {
                   return false;  
                }   
            }

            return true;
        }
    }
}