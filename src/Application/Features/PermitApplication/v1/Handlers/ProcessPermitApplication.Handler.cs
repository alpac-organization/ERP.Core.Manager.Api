using System.Text.Json;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Handlers
{
    public class ProcessPermitApplicationtHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IDeductionsServices _deductionServices, IReportingServices _reportingServices, IIncomeServices _incomeServices): AlpacBaseHandler<ProcessPermitApplicationCommand, bool>(_unitOfWork, _errorManager)
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
            
            #region Información del usuario

            var user = await _unitOfWork.Users.Entities
                .Where(u => u.Id == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return _errorManager.ThrowBadRequest<bool>("No se encontro el usuario asociado a la solicitud", "ERP:001");
            }

            #endregion

            #region Verificar si existe el permiso solicitado
            
            var permitApplication = await _unitOfWork.PermitApplications.Entities
                .Where(vr => vr.Id == request.PermitApplicationId)
                .Where(vr => vr.Status != PermitApplicationStatus.Cancelled)
                .Include(vr => vr.Collaborator)
                    .ThenInclude(vr => vr.WorkingInformation)
                .FirstOrDefaultAsync(cancellationToken);

            if (permitApplication is null)
            {
                return _errorManager.ThrowBadRequest<bool>("No se encontro la solicitud del colaborador", "ERP:001");
            }

            #endregion
            
            #region Control de vacaciones
            
            var vacationInformationSolicitante = await _unitOfWork.Vacations.Entities
                .Where(v => v.CollaboratorId == permitApplication.CollaboratorId)
                .FirstOrDefaultAsync(cancellationToken);

            if (vacationInformationSolicitante is null)
            {
                return _errorManager.ThrowBadRequest<bool>("😐No se encontro registro de vacaciones del solicitante", "ERP:02");
            }

            #endregion

            #region Información del tipo de salario

            var salaryInformation = await _unitOfWork.Salaries.Entities
                .Where(sal => sal.EndDate == null)
                .Where(sal => sal.CollaboratorId == permitApplication.Collaborator.Id)
                .Include(sal => sal.Collaborator)
                    .ThenInclude(sal => sal.WorkingInformation)
                .FirstOrDefaultAsync(cancellationToken);

            if (salaryInformation is null)
            {
                return _errorManager.ThrowBadRequest<bool>("😐No se encontro la información salarial del colaborador", "ERP:02");
            }

            #endregion

            switch (permitApplication.Type)
            {
                case PermitApplicationType.VacationPay:
                {
                    #region Primer proceso de aprobación
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

                    #region Segundo proceso de aprobación

                    var (isAuthorized, continueProcess) = ProcessSecondStepOfPermitApplication(permitApplication, access.Role!.RoleType, request.IsApproved, user.Fullname ?? "unknow user");

                    if (isAuthorized is false)
                    {
                        return _errorManager.ThrowBadRequest<bool>("No tienes permisos para realizar esta operación, no eres administrador", "ERP:03");
                    }

                    if(isAuthorized && continueProcess)
                    {
                        //Procesar pago de vacaciones
                        bool IsSuccess = await _incomeServices.ApplyVacationPay(permitApplication.Collaborator, salaryInformation, permitApplication.PayrolId, permitApplication?.AmountDays ?? 0.0m);

                        if (!IsSuccess)
                        {
                            return _errorManager.ThrowBadRequest<bool>("Ocurrio un error al procesar el pago de vacaciones", "ERP:04");
                        }
                        
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }

                    #endregion

                    return true;
                }
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
                    
                    var additionalInformation = JsonSerializer.Deserialize<AdditionalDataPermitApplication>(permitApplication.AdditionalData);

                    if(isAuthorized && continueProcess)
                    {

                        if (additionalInformation!.MedicalAppointmentData.IsFullDay)
                        {
                            vacationInformationSolicitante.AvailableVacations -= 0.5m;
                            vacationInformationSolicitante.EnjoyedVacation += 0.5m;

                            await _unitOfWork.Vacations.UpdateAsync(vacationInformationSolicitante);
                            await _reportingServices.ApplyVacationMovement(permitApplication.Collaborator, permitApplication.PayrolId);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                        }

                        await _unitOfWork.PermitApplications.UpdateAsync(permitApplication);

                        if (permitApplication.Status == PermitApplicationStatus.Approved && additionalInformation!.MedicalAppointmentData.IsFullDay)
                        {
                            await _deductionServices.ApplyDeductionTravelExpenses(permitApplication.Collaborator, salaryInformation, permitApplication.PayrolId);   
                        }

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

                        //Agregar que actualize la tabla vacations Accrual

                        var vacationAccrual = await _unitOfWork.VacationAccruals.Entities
                            .Where(va => va.PayrollId == permitApplication.PayrolId)
                            .Where(va => va.CollaboratorId == permitApplication.CollaboratorId)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (vacationAccrual is null)
                        {
                            return _errorManager.ThrowBadRequest<bool>("No se encontro el registro de vacaciones en la nomina", "ERP:01");
                        }

                        decimal salaryDaily = salaryInformation.AmountInLocal / 30.0m;
                        decimal vacationAmount = salaryDaily * vacationInformationSolicitante.AvailableVacations;

                        vacationAccrual.AvailableVacations = vacationInformationSolicitante.AvailableVacations;
                        vacationAccrual.FinalBalance = vacationInformationSolicitante.AvailableVacations;
                        vacationAccrual.EquivalentQuantity = vacationAmount;
                        vacationAccrual.EquivalentQuantityInDollars = vacationAmount / 36.6243m;                     

                        await _unitOfWork.VacationAccruals.UpdateAsync(vacationAccrual);
                    }
                    
                    await _unitOfWork.PermitApplications.UpdateAsync(permitApplication);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    if (permitApplication.Status is PermitApplicationStatus.Approved)
                    {
                        await _deductionServices.ApplyDeductionTravelExpenses(permitApplication.Collaborator, salaryInformation, permitApplication.PayrolId);
                        await _reportingServices.ApplyVacationMovement(permitApplication.Collaborator, permitApplication.PayrolId);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }

                    return true;
                    #endregion
                }
                default:
                {
                    return _errorManager.ThrowBadRequest<bool>("Este tipo de solicitud no se encuentra disponible", "ERP:001");
                }
            }
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
                permitApplication.ManagerFullname = string.IsNullOrEmpty(permitApplication.ManagerFullname) ?  userFullname : permitApplication.ManagerFullname;

                permitApplication.Status = PermitApplicationStatus.Rejected;

                continueProcess = false;
            }

            return (authorized, continueProcess);
        }
        #endregion
    }
}