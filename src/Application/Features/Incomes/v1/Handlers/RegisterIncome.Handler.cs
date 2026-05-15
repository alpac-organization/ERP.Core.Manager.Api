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

            var Income = await _unitOfWork.TypesIncome.Entities
                .Where(type => type.Id == request.TypeIncomeId && type.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            if (Income is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Este tipo de ingreso no se encuentra disponible!", "ERP:03");
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
                            .Where(col => col.IdentificationNumber == collaborator.IdentificationNumber && col.CompanyId == request.CompanyId && col.Status != CollaboratorStatus.Inactive)
                            .Include(col => col.WorkingInformation)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (collaboratorInformation is null)
                        {
                            logger.LogInformation("No se encontro al colaborador con cedula: {identificacion}", collaborator.IdentificationNumber);
                            continue;   
                        }
                        
                        logger.LogInformation("Agregando ingreso de horas extras a colaborador con cedula {identification}", collaboratorInformation.IdentificationNumber);
                        

                        var salaryInformation = await _unitOfWork.Salaries.Entities
                            .Where(col => col.CollaboratorId == collaboratorInformation.Id)
                            .Where(col => col.EndDate == null && col.SalaryType == SalaryType.Fixed)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (salaryInformation is null)
                        {
                            return _errorManager.ThrowBadRequest<bool>("No se pudo obtener la información salarial", "ERP:03");
                        }

                        var payroll = await _unitOfWork.Payrolls.Entities 
                            .Where(
                                pay => pay.Status == PayrollStatus.Progress && 
                                pay.Id == request.PayrollId
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
                        decimal AmountTotalWithHours = HourlyWage * collaborator.AmountHours * 2;

                        ordinaryPayrollInfo.Overtime        = AmountTotalWithHours;                    
                        ordinaryPayrollInfo.NumberOvertime  = collaborator.AmountHours;
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
                        ordinaryPayrollInfo.NumberOvertime   = collaborator.AmountHours;
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
                            Description = "Horas extras",                        
                        });

                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        logger.LogInformation("✅Se agrego con exito el registro de horas extras.");
                    }

                    return true;
                }
                case "COMMISSION":
                {
                    #region Control de validaciones

                    if (request.CommissionsPayload is null)                   return _errorManager.ThrowBadRequest<bool>("Los datos para registro de comisiones es requerido", "ERP:02");
                    if (request.CommissionsPayload.CommissionAmount <= 0)     return _errorManager.ThrowBadRequest<bool>("El monto de las comisiones no puede ser menor o igual a 0", "ERP:02");
                    if (!Enum.IsDefined(request.CommissionsPayload.Currency)) return _errorManager.ThrowBadRequest<bool>("La moneda es requerida", "ERP:02");

                    var collaboratorInformation = await _unitOfWork.Collaborators.Entities
                        .Where(col => col.IdentificationNumber == request.CommissionsPayload.IdentificationNumber && col.CompanyId == request.CompanyId && (col.Status != CollaboratorStatus.Inactive || col.Status !=  CollaboratorStatus.Subsidy))
                        .Include(col => col.WorkingInformation)
                            .ThenInclude(work => work.BranchInfo)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (collaboratorInformation is null)
                    {
                        return _errorManager.ThrowBadRequest<bool>("Este collaborador no existe", "ERP:01");
                    }

                    var salaryInformation = await _unitOfWork.Salaries.Entities
                        .Where(sal => sal.EndDate == null && sal.CollaboratorId == collaboratorInformation.Id)
                        .Include(sal => sal.Collaborator)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (salaryInformation is null)
                    {
                        return _errorManager.ThrowBadRequest<bool>("No se encontro la información salarial de este colaborador", "ERP:SalaryNotFound");
                    }

                    var payroll = await _unitOfWork.Payrolls.Entities 
                        .Where(
                            pay => pay.Status == PayrollStatus.Progress && 
                            pay.BranchId == collaboratorInformation.WorkingInformation.BranchInfo.Id &&
                            pay.Id == request.PayrollId
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

                    #endregion

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

                    decimal TotalIncome = ordinaryPayrollInfo.Antique + ordinaryPayrollInfo.Overtime + ordinaryPayrollInfo.Bonus + ordinaryPayrollInfo.BiweeklySalary;

                    TotalIncome += request.CommissionsPayload.CommissionAmount;         
                    ordinaryPayrollInfo.TotalIncome = TotalIncome;

                    var (BiweeklyInss, BiweeklyIr) = await _calculatorDeduction.CalculateIrToNextProcess(
                        lastFortnight ?? 24,
                        TaxInformation?.SalaryEarned       ?? 0.0m,
                        TaxInformation?.AccumulatedIR      ?? 0.0m,
                        TotalIncome,
                        cancellationToken
                    );

                    lastIncomeTax?.AccumulatedIR = BiweeklyIr;
                    lastIncomeTax?.SalaryEarned  = TotalIncome - BiweeklyInss;

                    //Actualizar datos de deducciones.
                    ordinaryPayrollInfo.Ir                   = BiweeklyIr;
                    ordinaryPayrollInfo.Inss                 = BiweeklyInss;
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

                    decimal total = ordinaryPayrollInfo.TotalIncome - BiweeklyInss - BiweeklyIr - totalDeductions + ordinaryPayrollInfo.TotalTravelExpenses;

                    ordinaryPayrollInfo.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);

                    await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayrollInfo);
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