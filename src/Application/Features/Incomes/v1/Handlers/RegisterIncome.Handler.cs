using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Incomes.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Incomes.v1.Handlers
{
    public class RegisterIncomeHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<RegisterIncomeHandler> logger, IIncomeServices _incomeServices): AlpacBaseHandler<RegisterIncomeCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(RegisterIncomeCommand request, CancellationToken cancellationToken)
        {
            #pragma warning disable CA1873 
            
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            if (access.Role!.RoleType != RoleType.Administrator)
            {
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para registrar una dedución", "ERP:01");
            }

            var Income = await _unitOfWork.TypesIncome.Entities
                .Where(type => type.Id == request.TypeIncomeId && type.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            if (Income is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Este tipo de ingreso no se encuentra disponible!", "ERP:03");
            }

            var payroll = await _unitOfWork.Payrolls.Entities 
                .Where(pay => pay.Id == request.PayrollId)
                .Where(pay => pay.BranchId == request.BranchId)
                .Where(pay => pay.Status == PayrollStatus.Progress)
                .FirstOrDefaultAsync(cancellationToken);

            if (payroll is null)
            {
                return _errorManager.ThrowBadRequest<bool>("No existe un periodo de nomina activo, apertura el periodo de nomina", "");
            }

            logger.LogInformation("🚩Iniciando proceso de ingreso\n");

            var IncomePayload = new Income();

            switch (Income.IncomeCode)
            {
                case "OVERTIME":
                {
                    foreach (var collaborator in request.OvertimeIncomeData)
                    {
                        var collaboratorInformation = await _unitOfWork.Collaborators.Entities
                            .Where(col => col.IdentificationNumber == collaborator.IdentificationNumber && col.CompanyId == request.CompanyId)
                            .Where(col => col.Status != CollaboratorStatus.Inactive)
                            .Include(col => col.WorkingInformation)
                            .Where(col => col.WorkingInformation.CompanyBranchId == request.BranchId)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (collaboratorInformation is null)
                        {
                            logger.LogInformation("No se encontro al colaborador con cedula: {identificacion}", collaborator.IdentificationNumber);
                            continue;   
                        }
                                                
                        var salaryInformation = await _unitOfWork.Salaries.Entities
                            .Where(col => col.CollaboratorId == collaboratorInformation.Id)
                            .Where(col => col.EndDate == null && col.SalaryType == SalaryType.Fixed)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (salaryInformation is null)
                        {
                            return _errorManager.ThrowBadRequest<bool>("No se pudo obtener la información salarial", "ERP:03");
                        }
                    
                        await _incomeServices.ApplyIncomeOvertime(collaboratorInformation, salaryInformation, collaborator.AmountHours, payroll.Id ,request.TypeIncomeId);

                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        logger.LogInformation("✅Se agrego con exito el registro de horas extras.");
                    }

                    return true;
                }
                case "COMMISSION":
                {
                    if (request.CommissionsPayload is null)                   
                        return _errorManager.ThrowBadRequest<bool>("Los datos para registro de comisiones es requerido", "ERP:02");
                    
                    if (request.CommissionsPayload.CommissionAmount <= 0)     
                        return _errorManager.ThrowBadRequest<bool>("El monto de las comisiones no puede ser menor o igual a 0", "ERP:02");

                    if (string.IsNullOrEmpty(request.CommissionsPayload.IdentificationNumber))     
                        return _errorManager.ThrowBadRequest<bool>("El número de identificación es requerido", "ERP:02");

                    if (!Enum.IsDefined(request.CommissionsPayload.Currency)) 
                        return _errorManager.ThrowBadRequest<bool>("La moneda es requerida", "ERP:02");

                    var collaboratorInformation = await _unitOfWork.Collaborators.Entities
                        .Include(col => col.WorkingInformation)
                        .Where(col => col.IdentificationNumber == request.CommissionsPayload.IdentificationNumber 
                            && col.CompanyId == request.CompanyId 
                            && col.Status != CollaboratorStatus.Inactive 
                            && col.Status != CollaboratorStatus.Subsidy)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (collaboratorInformation is null)
                    {
                        return _errorManager.ThrowBadRequest<bool>("Este collaborador no existe", "ERP:01");
                    }

                    var salaryInformation = await _unitOfWork.Salaries.Entities
                        
                        .Where(sal => sal.EndDate == null && sal.CollaboratorId == collaboratorInformation.Id)
                        .Include(sal => sal.Collaborator)
                            .ThenInclude(sal => sal.WorkingInformation)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (salaryInformation is null)
                    {
                        return _errorManager.ThrowBadRequest<bool>("No se encontro la información salarial de este colaborador", "ERP:SalaryNotFound");
                    }
                    
                    await _incomeServices.ApplyIncomeCommissions(collaboratorInformation, salaryInformation, request.CommissionsPayload.CommissionAmount, payroll.Id, request.TypeIncomeId);
                   
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    logger.LogInformation("Se agrego con exito el registro de comisiones"); 
                    return true;
                }
                default:
                {
                    return _errorManager.ThrowBadRequest<bool>("Este tipo de ingreso no esta disponible", "ERP:04");
                }
            }

            #pragma warning restore CA1873
        }
    }
}