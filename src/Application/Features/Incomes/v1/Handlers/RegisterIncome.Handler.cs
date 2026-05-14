using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Incomes.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using System.Text.Json;

namespace ERP.Core.Manager.Api.Application.Features.Incomes.v1.Handlers
{
    public class RegisterIncomeHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<RegisterIncomeHandler> logger, ICalculatorDeductions _calculatorDeduction): AlpacBaseHandler<RegisterIncomeCommand, bool>(_unitOfWork, _errorManager)
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

            var collaboratorInformation = await _unitOfWork.Collaborators.Entities
                .Where(col => col.IdentificationNumber == request.IdentificationNumber && col.CompanyId == request.CompanyId && (col.Status != CollaboratorStatus.Inactive || col.Status !=  CollaboratorStatus.Subsidy))
                .Include(col => col.WorkingInformation)
                    .ThenInclude(work => work.BranchInfo)
                .FirstOrDefaultAsync(cancellationToken);

            if (collaboratorInformation is null)
            {

                return _errorManager.ThrowBadRequest<bool>("Este collaborador no existe", "ERP:01");
            }

            var payroll = await _unitOfWork.Payrolls.Entities 
                .Where(
                    pay => pay.Status == PayrollStatus.Progress && 
                    pay.BranchId == collaboratorInformation.WorkingInformation.BranchInfo.Id
                )
                .FirstOrDefaultAsync(cancellationToken);

            if (payroll is null)
            {
                logger.LogInformation("No se encontro una nomina en progreso");
                return _errorManager.ThrowBadRequest<bool>("No existe un periodo de nomina activo, apertura el periodo de nomina", "");
            }
            
            var ordinaryPayrollInfo = await _unitOfWork.OrdinaryPayrolls.Entities
                .Where(ord => ord.CollaboratorId == collaboratorInformation.Id && ord.PayrollId == payroll.Id)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (ordinaryPayrollInfo is null)
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


            var salaryInformation = await _unitOfWork.Salaries.Entities
                .Where(sal => sal.EndDate == null && sal.CollaboratorId == collaboratorInformation.Id)
                .Include(sal => sal.Collaborator)
                .FirstOrDefaultAsync(cancellationToken);

            if (salaryInformation is null)
            {
                return _errorManager.ThrowBadRequest<bool>("No se encontro la información salarial de este colaborador", "ERP:SalaryNotFound");
            }

            logger.LogInformation("🚩Iniciando proceso de ingreso\n");

            var IncomePayload = new Income();

            switch (Income.IncomeCode)
            {
                case "OVERTIME":
                {
                    logger.LogInformation("Agregando ingreso de horas extras a colaborador con cedula {identification}", collaboratorInformation.IdentificationNumber);

                    decimal DailySalary = salaryInformation.AmountInLocal / 30;
                    decimal HourlyWage = DailySalary / 8;
                    
                    int daysWorked = 15;
                    DateTime entryDate = salaryInformation.Collaborator.WorkingInformation.EntryDate;
                    DateTime payrollStart = salaryInformation.StartDate;

                    DateTime payrollEnd = payroll.EndDate ?? payrollStart.AddDays(14);

                    if (entryDate > payrollStart) daysWorked = (payrollEnd - entryDate).Days + 1;
                    else  daysWorked = 15;

                    if (daysWorked < 0) daysWorked = 0;
                    if (daysWorked > 15) daysWorked = 15;

                    decimal ProportionalBiweeklySalary = DailySalary * daysWorked;
                    decimal AmountTotalWithHours = (HourlyWage * request.OvertimeIncomePayload?.AmountHours ?? 0) * 2;

                    ordinaryPayrollInfo.Overtime        = AmountTotalWithHours;                    
                    ordinaryPayrollInfo.NumberOvertime  = request.OvertimeIncomePayload?.AmountHours ?? 0;
                    ordinaryPayrollInfo.TotalIncome     = ordinaryPayrollInfo.Bonus + ordinaryPayrollInfo.Commissions + AmountTotalWithHours + ProportionalBiweeklySalary + ordinaryPayrollInfo.Antique;

                    decimal GrossSalary = ordinaryPayrollInfo.TotalIncome;

                    var lastIncomeTax = await _unitOfWork.IncomeTaxAccrual.Entities
                        .Where(income => income.CollaboratorId == salaryInformation.Collaborator.Id && income.PayrollId == request.PayrollId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (lastIncomeTax is null)
                    {
                        return _errorManager.ThrowBadRequest<bool>("No se puedo encontrar el ultimo registro acumulados del colaborador", "ERP:IncomeTaxNotFound");
                    }

                    var lastFortnight = lastIncomeTax?.NumberOfFortnights + 1;
                    if (lastFortnight is 25) lastFortnight = 24;


                    var TaxInformation = await _unitOfWork.IncomeTaxAccrual.Entities
                        .Where(income => income.CollaboratorId == salaryInformation.Collaborator.Id && income.NumberOfFortnights == lastFortnight)
                        .OrderByDescending(income => income.CreatedAt)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (TaxInformation is null)
                    {
                        return _errorManager.ThrowBadRequest<bool>("No se encontro el control de acumulados para este colaborador", "ERP:TaxInformationNotFound");
                    }
                    
                    logger.LogInformation("Calculando inss e ir");

                    var (BiweeklyInss, BiweeklyIr) = await _calculatorDeduction.CalculateIrToNextProcess(
                        lastFortnight ?? 24,
                        TaxInformation?.SalaryEarned       ?? 0.0m,
                        TaxInformation?.AccumulatedIR      ?? 0.0m,
                        GrossSalary,
                        cancellationToken
                    );

                    //Actualiza acumulado
                    lastIncomeTax?.AccumulatedIR = BiweeklyIr;
                    lastIncomeTax?.SalaryEarned  = GrossSalary - BiweeklyInss;

                    //Actualizar datos de deducciones.
                    ordinaryPayrollInfo.Ir      = BiweeklyIr;
                    ordinaryPayrollInfo.Inss    = BiweeklyInss;
                    ordinaryPayrollInfo.TotalLegalDeductions = BiweeklyInss + BiweeklyIr;

                     var deductions =
                        JsonSerializer.Deserialize<DeductionsAdditionalData>(
                            ordinaryPayrollInfo.DeductionsAdditionalData
                        ) ?? new DeductionsAdditionalData();

                    decimal totalDeductions =
                        deductions.Loans
                        + deductions.Purisima
                        + deductions.ChildSupportGarnishment
                        + deductions.SalaryAdvance
                        + deductions.ChristmasBonusAdvance
                        + deductions.JudicialSeizures
                        + deductions.UniformDeduction
                        + deductions.CashShortage
                        + deductions.OtherDeductions
                        + deductions.DeductionForLossesBulk
                        + deductions.Absences
                        + deductions.Sanction
                        + deductions.LateArrivals;

                    decimal total = ordinaryPayrollInfo.TotalIncome - BiweeklyInss - BiweeklyIr - totalDeductions;
                    
                    ordinaryPayrollInfo.TotalToPay = total + ordinaryPayrollInfo.Transport + ordinaryPayrollInfo.Lodging + ordinaryPayrollInfo.Feeding;
                    ordinaryPayrollInfo.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);
                    
                    ordinaryPayrollInfo.GrossSalary      = salaryInformation.AmountInLocal / 2;
                    ordinaryPayrollInfo.NumberOvertime   = request.OvertimeIncomePayload?.AmountHours ?? 0;
                    ordinaryPayrollInfo.TotalDeducctions = totalDeductions + BiweeklyIr + BiweeklyInss;

                    //Actualizamos su acumulado
                    await _unitOfWork.IncomeTaxAccrual.UpdateAsync(lastIncomeTax!);


                    //Actualizamos la nomina
                    await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayrollInfo);


                    await _unitOfWork.Incomes.RegisterIncome(new()
                    {
                        Currency = Currency.NIO,
                        AmountInLocal = AmountTotalWithHours,
                        AmountInDollars = AmountTotalWithHours / 36.6243m,
                        CollaboratorId = salaryInformation.Collaborator.Id,
                        IncomeTypeId = request.TypeIncomeId,
                        PayrollId = payroll.Id,
                        Description = request.Description,                        
                    });


                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    logger.LogInformation("✅Se agrego con exito el registro de horas extras.");
                    return true;
                }
                case "COMMISSION":
                {
                    logger.LogInformation("Agregando Ingreso de comisiones");

                    var lastIncomeTax = await _unitOfWork.IncomeTaxAccrual.Entities
                        .Where(income => income.CollaboratorId == salaryInformation.Collaborator.Id && income.PayrollId == request.PayrollId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (lastIncomeTax is null)
                    {
                        return _errorManager.ThrowBadRequest<bool>("No se puedo encontrar el ultimo registro acumulados del colaborador", "ERP:IncomeTaxNotFound");
                    }

                    var lastFortnight = lastIncomeTax?.NumberOfFortnights + 1;
                    if (lastFortnight is 25) lastFortnight = 24;

                    var TaxInformation = await _unitOfWork.IncomeTaxAccrual.Entities
                        .Where(income => income.CollaboratorId == salaryInformation.Collaborator.Id && income.NumberOfFortnights == lastFortnight)
                        .OrderByDescending(income => income.CreatedAt)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (TaxInformation is null)
                    {
                        return _errorManager.ThrowBadRequest<bool>("No se encontro el control de acumulados para este colaborador", "ERP:TaxInformationNotFound");
                    }
                    
                    int daysWorked = 15;
                    DateTime entryDate = salaryInformation.Collaborator.WorkingInformation.EntryDate;
                    DateTime payrollStart = salaryInformation.StartDate;

                    DateTime payrollEnd = payroll.EndDate ?? payrollStart.AddDays(14);

                    if (entryDate > payrollStart) daysWorked = (payrollEnd - entryDate).Days + 1;
                    else  daysWorked = 15;

                    if (daysWorked < 0) daysWorked = 0;
                    if (daysWorked > 15) daysWorked = 15;

                    


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