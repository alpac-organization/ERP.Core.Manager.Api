using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Incomes.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Incomes.v1.Handlers
{
    public class RegisterIncomeHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager): AlpacBaseHandler<RegisterIncomeCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(RegisterIncomeCommand request, CancellationToken cancellationToken)
        {

            #pragma warning disable CA1873 // Avoid potentially expensive logging
            
            // logger.LogInformation("data: {data}", JsonSerializer.Serialize(request));
            
            #pragma warning restore CA1873 // Avoid potentially expensive logging

            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            if (access.Role!.RoleType != RoleType.Administrator)
            {
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para registrar una dedución", "ERP:01");
            }

            // logger.LogInformation("Iniciando proceso de registro de ingreso");

            var collaboratorInformation = await _unitOfWork.Collaborators.Entities
                .Where(col => col.IdentificationNumber == request.IdentificationNumber && col.CompanyId == request.CompanyId)
                .Include(col => col.WorkingInformation)
                    .ThenInclude(work => work.BranchInfo)
                .FirstOrDefaultAsync(cancellationToken);

            if (collaboratorInformation is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Este collaborador no existe", "ERP:01");
            }

            //Obtener la nomina del la sucursal a la que pertence
            var payroll = await _unitOfWork.Payrolls.Entities 
                .Where(pay => pay.Status == PayrollStatus.Progress && pay.BranchId == collaboratorInformation.WorkingInformation.BranchInfo.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (payroll is null)
            {
                // logger.LogInformation("No se encontro una nomina en progreso");
                return false;
            }
            else
            {

                var ordinaryPayrollInformation = await _unitOfWork.OrdinaryPayrolls.Entities
                    .Where(ord => ord.CollaboratorId == collaboratorInformation.Id && ord.PayrollId == payroll.Id)
                    .FirstOrDefaultAsync(cancellationToken);


                if (ordinaryPayrollInformation is null)
                {
                    return _errorManager.ThrowNotFound<bool>("No se encontro registro del colaborador en la nomina", "ERP:02");
                }

                //Verificarel si ese ingreso esta disponible
                var Income = await _unitOfWork.TypesIncome.Entities
                    .Where(type => type.Id == request.TypeIncomeId && type.IsActive)
                    .FirstOrDefaultAsync(cancellationToken);

                if (Income is null)
                {
                    return _errorManager.ThrowBadRequest<bool>("Este tipo de ingreso no se encuentra disponible!", "ERP:03");
                }
                //Iniciar Proceso de registro de ingreso.

                var IncomePayload = new Income()
                {
                    CollaboratorId  = collaboratorInformation.Id,
                    AmountInLocal   = request.IncomeAmount,
                    Description     = request.Description,
                    AmountInDollars = request.IncomeAmount / 36.6243m,
                    Currency        = Currency.NIO,
                    IncomeTypeId    = request.TypeIncomeId,
                    PayrollId       = payroll.Id                    
                };

                await _unitOfWork.Incomes.RegisterIncome(IncomePayload);

                // Iniciando proceso de ingreso y contabilidad de nomina.
                switch (Income.IncomeCode)
                {
                    case "ALW_MEAL" :
                    {
                        // logger.LogInformation("Agregando ingreso de alimentación a nomina");

                        ordinaryPayrollInformation.FoodTravelAllowance = request.IncomeAmount;
                        ordinaryPayrollInformation.TotalToPay += request.IncomeAmount;
                        
                        await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayrollInformation);

                        // logger.LogInformation("Proceso finalizado con exito!"); 
                        
                        return true;
                    }
                    case "ALW_HOUSING" :
                    {

                        // logger.LogInformation("Agregando ingreso de hospedaje a nomina");

                        ordinaryPayrollInformation.Lodging = request.IncomeAmount;
                        ordinaryPayrollInformation.TotalToPay += request.IncomeAmount;
                        
                        await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayrollInformation);

                        // logger.LogInformation("Proceso finalizado con exito!");

                        return true;
                    }
                    case "ALW_TRANSPORT":
                    {

                        // logger.LogInformation("Agregando ingreso de transporte a nomina");

                        ordinaryPayrollInformation.Lodging = request.IncomeAmount;
                        ordinaryPayrollInformation.TotalToPay += request.IncomeAmount;
                        
                        await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayrollInformation);

                        // logger.LogInformation("Proceso finalizado con exito!");

                        return true;
                    }
                    default:
                    {
                        _errorManager.ThrowBadRequest<bool>("Este tipo de ingreso no esta disponible", "ERP:04");
                        break;   
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true; 
        }
    }
}