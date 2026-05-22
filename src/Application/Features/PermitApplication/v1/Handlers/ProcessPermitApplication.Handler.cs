using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using System.Text.Json;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using System.Reflection.Metadata;
using ERP.Core.Database.Domain.Entities.Auth;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Handlers
{
    public class ProcessPermitApplicationtHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager): AlpacBaseHandler<ProcessPermitApplicationCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(ProcessPermitApplicationCommand request, CancellationToken cancellationToken)
        {

            #region Verificar acceso al modulo
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse; 
            }

            #endregion

            #region Verificar si existe el permiso solicitado
            var permitApplication = await _unitOfWork.PermitApplications.Entities
                .Where(vr => vr.Id == request.PermitApplicationId)
                .Where(vr => vr.Status != PermitApplicationStatus.Cancelled)
                .FirstOrDefaultAsync(cancellationToken);

            if (permitApplication is null)
            {
                return _errorManager.ThrowBadRequest<bool>("No se encontro la solicitud del colaborador", "ERP:001");
            }
            #endregion

            var user = await _unitOfWork.Users.Entities
                .Where(u => u.Id == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return _errorManager.ThrowBadRequest<bool>("No se encontro el usuario asociado a la solicitud", "ERP:001");
            }

            var collaboratorInformation = await _unitOfWork.Collaborators.Entities
                .Where(c => c.Id == permitApplication.CollaboratorId)
                .Where(c => c.CompanyId == request.CompanyId)
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
                #region Proceso de pago de vaciones en el proceso de donaciones de vacaciones
                // case PermitApplicationType.DonatedVacations:
                // {
                //     #region Primero proceso de aprobación
                //     var response = MapperInformationToApprovedFirstStep(
                //         permitApplication, 
                //         access.Role!.RoleType, 
                //         request.IsApproved, 
                //         user.Fullname ?? "unknow user"
                //     );

                //     if (response)
                //     {
                //         await _unitOfWork.PermitApplications.UpdateAsync(permitApplication);
                //         await _unitOfWork.SaveChangesAsync(cancellationToken);
                //     }                            
                //     else return _errorManager.ThrowBadRequest<bool>("No tienes permiso para aprobar esta solicitud", "ERP:01"); 
                //     #endregion

                //     #region Aprobación de solicitud del colaborador
                //     if ((permitApplication.FirtsStepApproved is true || permitApplication.FirtsStepApproved is false) && request.IsApproved)
                //     {
                //         if (access.Role!.RoleType == RoleType.Manager && collaboratorInformation)
                //         {
                //             return _errorManager.ThrowBadRequest<bool>("No puedes aprobarte el proceso, no eres administrador", "ERP:03");
                //         }

                //         if (access.Role!.RoleType == RoleType.Administrator)
                //         {
                //             var vacationControlToReceive = await _unitOfWork.Vacations.Entities
                //                 .Include(vac  => vac.Collaborator)
                //                 .Where(vac  => vac.Collaborator.IdentificationNumber == permitApplication.IdentificationCollaboratorToReceive)
                //                 .FirstOrDefaultAsync(cancellationToken);

                //             if (vacationControlToReceive is null)
                //             {
                //                 return _errorManager.ThrowBadRequest<bool>("El colaborador que recibira las vacaciones no tiene proceso de vacaciones", "ERP:03");                            
                //             }

                //             //Reducimos las vacaciones para el solicitante.
                //             vacationInformationSolicitante.AvailableVacations -= permitApplication.AmountDays ?? 0m;
                //             vacationInformationSolicitante.EnjoyedVacation += permitApplication.AmountDays ?? 0;
                            
                //             await _unitOfWork.Vacations.UpdateAsync(vacationInformationSolicitante);
                //             await _unitOfWork.SaveChangesAsync(cancellationToken);


                //             //Le aumentamos la vacaciones a la persona que recibira las vacaciones.
                //             vacationControlToReceive.AvailableVacations += permitApplication.AmountDays ?? 0m;
                //             vacationControlToReceive.DonatedVacation += permitApplication.AmountDays ?? 0m;

                //             await _unitOfWork.Vacations.UpdateAsync(vacationControlToReceive);
                //             await _unitOfWork.SaveChangesAsync(cancellationToken);

                //             permitApplication.AdministratorFullName = $"{user.Fullname}";
                //             permitApplication.SecondStepApproved = true;
                //             permitApplication.Status = PermitApplicationStatus.Approved;

                //             await _unitOfWork.PermitApplications.UpdateAsync(permitApplication);
                //             await _unitOfWork.SaveChangesAsync(cancellationToken);

                //             return true;
                //         }
                //         else
                //         {
                //             return _errorManager.ThrowBadRequest<bool>("Solo administradores pueden aprobar el ultimo proceso de solicitud", "ERP:02");   
                //         }
                //     }
                //     #endregion

                //     #region Rechazar Solicitud de colaborador
                //     else if((permitApplication.FirtsStepApproved is true || permitApplication.FirtsStepApproved is false) && request.IsApproved is false)
                //     {

                //         if (access.Role!.RoleType == RoleType.Manager && collaboratorInformation)
                //         {
                //             return _errorManager.ThrowBadRequest<bool>("No puedes rechazar el proceso, no eres administrador", "ERP:03");
                //         }

                //         var IsSuccess = RejectPermitApplication(access.Role.RoleType, permitApplication, user.Fullname!);

                //         if (IsSuccess)
                //         {
                //             await _unitOfWork.PermitApplications.UpdateAsync(permitApplication);
                //             await _unitOfWork.SaveChangesAsync(cancellationToken);
                //         } else return _errorManager.ThrowBadRequest<bool>("Solo administradores pueden cancelar la solicitud", "ERP:03"); 
                //     }
                //     #endregion

                //     break;
                // }
                #endregion
                case PermitApplicationType.MedicalAppointment:
                {
                    #region Primero proceso de aprobación
                    
                    var (authorized, updateFirstStep) = ProcessFirstStepOfPermitApplication(permitApplication, access.Role!.RoleType, request.IsApproved, user.Fullname ?? "unknow user");

                    if (authorized is false)
                    {
                        return _errorManager.ThrowBadRequest<bool>("No tienes permiso para aprobar o rechazar esta solicitud", "ERP:01");        
                    }
                    else if (authorized && updateFirstStep)
                    {
                        await _unitOfWork.PermitApplications.UpdateAsync(permitApplication);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        return false;
                    }

                    #endregion

                    #region Segundo paso de aprobación de solicitud de colaborador

                    var (isAuthorized, continueProcess) = ProcessSecondStepOfPermitApplication(permitApplication, access.Role!.RoleType, request.IsApproved, user.Fullname ?? "unknow user");

                    if (isAuthorized is false)
                    {
                        return _errorManager.ThrowBadRequest<bool>("No tienes permisos para realizar esta operación, no eres administrador", "ERP:03");
                    }
                    
                    if(isAuthorized && continueProcess)
                    {
                        //Realizamos las mismas operaciones de calculo en este proceso

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

                    await _unitOfWork.PermitApplications.UpdateAsync(permitApplication);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                    return true;

                    #endregion                
                }
                case PermitApplicationType.Vacation:
                {
                    #region Primero proceso de aprobación
                    
                    var (authorized, updateFirstStep) = ProcessFirstStepOfPermitApplication(permitApplication, access.Role!.RoleType, request.IsApproved, user.Fullname ?? "unknow user");

                    if (authorized is false)
                    {
                        return _errorManager.ThrowBadRequest<bool>("No tienes permiso para aprobar o rechazar esta solicitud", "ERP:01");        
                    }
                    else if (authorized && updateFirstStep)
                    {
                        await _unitOfWork.PermitApplications.UpdateAsync(permitApplication);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        return false;
                    }

                    #endregion

                    #region Segundo paso de aprobación de solicitud de colaborador

                    var (isAuthorized, continueProcess) = ProcessSecondStepOfPermitApplication(permitApplication, access.Role!.RoleType, request.IsApproved, user.Fullname ?? "unknow user");

                    if (isAuthorized is false)
                    {
                        return _errorManager.ThrowBadRequest<bool>("No tienes permisos para realizar esta operación, no eres administrador", "ERP:03");
                    }
                    
                    if(isAuthorized && continueProcess)
                    {
                        //Relizamos la deducciones respectivas al colaborador de viaticos y vacaciones.
                        vacationInformationSolicitante.AvailableVacations -= permitApplication.AmountDays ?? 0.0m;
                        vacationInformationSolicitante.EnjoyedVacation    += permitApplication.AmountDays ?? 0.0m;

                        await _unitOfWork.Vacations.UpdateAsync(vacationInformationSolicitante);

                        //Aqui cambiar el estado del colaborador que esta solictando las vacaciones y fue aprobada dichas vacaciones.

                    }

                    await _unitOfWork.PermitApplications.UpdateAsync(permitApplication);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                    return true;
                    #endregion
                }
                default:
                {
                    return _errorManager.ThrowBadRequest<bool>("Este tipo de solicitud no se encuentra disponible", "ERP:001");
                }
            }
        }

        private static bool RejectPermitApplication(RoleType roleType, Database.Domain.Entities.Payrolls.PermitApplication permitApplication, string userFullname)
        {
            if (roleType == RoleType.Administrator )
            {
                permitApplication.AdministratorFullName = $"{userFullname}";
                permitApplication.SecondStepApproved = false;
                permitApplication.Status = PermitApplicationStatus.Rejected;

                return true;
            }
            else if(roleType == RoleType.Manager)
            {
                permitApplication.ManagerFullname = $"{userFullname}";
                permitApplication.FirtsStepApproved = false;

                return true;
            }
            else return false;
        }


        #region Función Manejadora de aprobar, rechazar el primer paso del traking.
        private static (bool, bool) ProcessFirstStepOfPermitApplication(Database.Domain.Entities.Payrolls.PermitApplication permitApplication, RoleType roleType, bool isApproved, string userFullname)
        {
            bool authorized   = true;
            bool updateFirstStep = false;

            //Jefe directo aprueba la solicitud realizada por el colaborador
            if (permitApplication.FirtsStepApproved is null && isApproved is true)
            {
                if (roleType != RoleType.Supervisor || roleType != RoleType.Operator)
                {
                    permitApplication.FirtsStepApproved = true;
                    permitApplication.ManagerFullname = $"{userFullname}";

                    updateFirstStep = true;

                    return (authorized, updateFirstStep);
                }

                authorized = false;

                return (authorized, updateFirstStep);
            }
            //Jefe directo rechaza la solicitud de realizada por el colaborador
            else if(permitApplication.FirtsStepApproved is null && isApproved is false)
            {
                if (roleType != RoleType.Supervisor || roleType != RoleType.Operator)
                {
                    permitApplication.FirtsStepApproved = false;
                    permitApplication.ManagerFullname = $"{userFullname}";

                    updateFirstStep = true;

                    return (authorized, updateFirstStep);
                }

                authorized = false;

                return (authorized, updateFirstStep);  
            }

            return (authorized, updateFirstStep);
        }
        #endregion

        #region Función Menajadora de rechar o reaprobar el primer paso del traking

        private static (bool, bool) ProcessSecondStepOfPermitApplication(Database.Domain.Entities.Payrolls.PermitApplication permitApplication, RoleType roleType, bool isApproved, string userFullname)
        {
            bool authorized = true;
            bool continueProcess = false;

            if (roleType != RoleType.Administrator)
            {
                authorized = false;
                return (authorized, continueProcess);
            }

            //No importa si el jefe directo rechazo la solicitud, puede reaprobar la solicitud
            if ((permitApplication.FirtsStepApproved is true || permitApplication.FirtsStepApproved is false) && isApproved)
            {
                //Caso de que el administrado haya aprobado la solicitud
                permitApplication.FirtsStepApproved = true;
                permitApplication.ManagerFullname = userFullname;
                permitApplication.SecondStepApproved = true;
                permitApplication.AdministratorFullName = userFullname;

                permitApplication.Status = PermitApplicationStatus.Approved;

                continueProcess = true;
            }
            
            if ((permitApplication.FirtsStepApproved is true || permitApplication.FirtsStepApproved is false) && isApproved is false)
            {
                //Caso de que el administrado haya rechazado la solictud
                permitApplication.SecondStepApproved = false;
                permitApplication.AdministratorFullName = userFullname;

                permitApplication.Status = PermitApplicationStatus.Rejected;

                continueProcess = false;
            }

            return (authorized, continueProcess);
        }
        #endregion
    }
}