using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Commands;


namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    #pragma warning disable CA1873

    public class IncomeServices(IUnitOfWork _unitOfWork,ICalculatorDeductions _calculatorDeductions, ILogger<CalculatorDeductions> _logger) : IIncomeServices
    {

        public async Task ApplyMedicalSubsidy(Collaborator collaboratorInformation, Salary salaryInformation,Payroll period, RegisterSubsidyCommmand data)
        {
            _logger.LogInformation("🚩Iniciando proceso de subsidio para el colaborador: {identification}", collaboratorInformation.IdentificationNumber);

            var taxIncome = await _unitOfWork.IncomeTaxAccrual.Entities
                .Where(tax => tax.PayrollId == period.Id)
                .Where(tax => tax.CollaboratorId == collaboratorInformation.Id)
                .FirstOrDefaultAsync(default);

            if (taxIncome is null)
            {
                _logger.LogInformation("El control de acumulado del colaborador con cedula {identification} no fue encontrado", collaboratorInformation.IdentificationNumber);
                return;
            }

            decimal monthlySalary = salaryInformation.AmountInLocal;
            decimal dailySalary = monthlySalary / 30;

            var informationPayroll = await _unitOfWork.OrdinaryPayrolls.Entities
                .Include(ord => ord.Payroll)
                .Where(ord => ord.CollaboratorId == collaboratorInformation.Id)
                .Where(ord => ord.PayrollId == period.Id)
                .FirstOrDefaultAsync(default);

            if (informationPayroll is null)
            {
                _logger.LogInformation("No se la información contable de la nomina");
                return;
            }

            #region Iniciar proceso de calculo de dias de subsidio dentro de la nomina

            DateTime payrollStartDate = period.StartDate.Date;
            DateTime payrollEndDate   = period.EndDate.Date;

            DateTime subsidyStartDate = data.StartDate.Date;
            DateTime subsidyEndDate   = data.EndDate.Date;

            DateTime effectiveStart = subsidyStartDate;
            DateTime effectiveEnd   = subsidyEndDate > payrollEndDate
                ? payrollEndDate
                : subsidyEndDate;

            if (effectiveEnd < effectiveStart)
            {
                throw new Exception("La fecha final del subsidio es inválida.");
            }

            int subsidizedDays = (effectiveEnd - effectiveStart).Days + 1;
            int daysWithoutSubsidy = 15 - subsidizedDays;

            // A: 
            decimal proportionalSalaryWithoutSubsidy = dailySalary * daysWithoutSubsidy;
            proportionalSalaryWithoutSubsidy += informationPayroll.Antique + informationPayroll.Overtime + informationPayroll.Bonus + informationPayroll.Antique;

            // B:
            decimal proportionalSalaryWithSubsidy = dailySalary * subsidizedDays;

            //Sacar el 40% del pago de salario a los dias subsidiados
            decimal inssWithSubsidy = await _calculatorDeductions.CalculateInss(proportionalSalaryWithSubsidy, default);

            decimal GrossSalaryWithSubsidy = proportionalSalaryWithSubsidy - inssWithSubsidy;

            //Sacar el 40% a los dias no subsidiados
            decimal GrossSalaryWithoutSubsidy = proportionalSalaryWithoutSubsidy * 0.4m;

            decimal TotalGrossSalary = GrossSalaryWithSubsidy + GrossSalaryWithoutSubsidy;

            //Aplicamos inss.
            var (BiweeklyInss, BiweeklyIr) = await _calculatorDeductions.CalculateIr(
                taxIncome?.NumberOfFortnights   ?? 24,
                taxIncome?.SalaryEarned         ?? 0,
                taxIncome?.AccumulatedIR        ?? 0,
                TotalGrossSalary,
                default,
                true
            );

            informationPayroll.Inss = inssWithSubsidy;
            informationPayroll.Ir = BiweeklyIr;

            informationPayroll.TotalLegalDeductions = inssWithSubsidy + BiweeklyIr;

            taxIncome?.FlagSalaryEarned += TotalGrossSalary;
            taxIncome?.FlagAccumulatedIR += BiweeklyIr;
            
            informationPayroll.TotalLegalDeductions = inssWithSubsidy + BiweeklyIr;


            var deductions =
                JsonSerializer.Deserialize<DeductionsAdditionalData>(
                    informationPayroll.DeductionsAdditionalData
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

            
            informationPayroll.TotalDeducctions = informationPayroll.TotalLegalDeductions + totalDeductions;
            informationPayroll.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);

            informationPayroll.TotalToPay = informationPayroll.TotalIncome - informationPayroll.TotalDeducctions;

            await _unitOfWork.IncomeTaxAccrual.UpdateAsync(taxIncome!);

            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(informationPayroll);

            _logger.LogInformation("✅Subsidio aplicado con exito.");
            #endregion 
        }


        public async Task ApplyIncomeBonus(Collaborator collaboratorInformation, Salary salaryInformation, decimal amountBonus, Currency currency, Guid payrollId, Guid incomeTypeId)
        {
            var ordinaryPayrollInfo = await _unitOfWork.OrdinaryPayrolls.Entities
                .Where(ord => ord.PayrollId == payrollId)
                .Where(ord => ord.CollaboratorId == collaboratorInformation.Id)
                .FirstOrDefaultAsync(default);

            if (ordinaryPayrollInfo is null)
            {
                _logger.LogInformation("No se encontro registro del colaborador con identificación {identification} en la nomina", collaboratorInformation.IdentificationNumber);
                return;
            }
            
            var lastIncomeTax = await _unitOfWork.IncomeTaxAccrual.Entities
                .Where(income => income.CollaboratorId == collaboratorInformation.Id && income.PayrollId == payrollId)
                .FirstOrDefaultAsync(default);

            if (lastIncomeTax is null)
            {
                _logger.LogInformation("No se puedo encontrar el ultimo registro acumulados del colaborador");
                return;
            }
            

        }

        public async Task ApplyIncomeOvertime(Collaborator collaboratorInformation, Salary salaryInformation, decimal totalHours, Guid payrollId, Guid typeIncomeId)
        {
            var ordinaryPayrollInfo = await _unitOfWork.OrdinaryPayrolls.Entities
                .Include(ord => ord.Payroll)
                .Where(ord => ord.PayrollId == payrollId)
                .Where(ord => ord.CollaboratorId == collaboratorInformation.Id)
                .FirstOrDefaultAsync(default);
                
            if (ordinaryPayrollInfo is null)
            {
                _logger.LogInformation("No se encontro registro del colaborador con identificación {identification} en la nomina", collaboratorInformation.IdentificationNumber);
                return;
            }

            decimal DailySalary = salaryInformation.AmountInLocal / 30;
            decimal HourlyWage = DailySalary / 8;
                        
            int daysWorked = 15;
            DateTime entryDate  = salaryInformation.Collaborator.WorkingInformation.EntryDate;
            DateTime payrollStart = salaryInformation.StartDate;

            DateTime payrollEnd = ordinaryPayrollInfo.Payroll.EndDate;

            if (entryDate > payrollStart) daysWorked = (payrollEnd - entryDate).Days + 1;
            else  daysWorked = 15;

            if (daysWorked < 0) daysWorked = 0;
            if (daysWorked > 15) daysWorked = 15;

            decimal ProportionalBiweeklySalary = DailySalary * daysWorked;
            decimal AmountTotalWithHours = HourlyWage * totalHours * 2;

            ordinaryPayrollInfo.Overtime        = AmountTotalWithHours;                    
            ordinaryPayrollInfo.NumberOvertime  = totalHours;
            ordinaryPayrollInfo.TotalIncome     = ordinaryPayrollInfo.Bonus + ordinaryPayrollInfo.Commissions + AmountTotalWithHours + ProportionalBiweeklySalary + ordinaryPayrollInfo.Antique;

            decimal GrossSalary = ordinaryPayrollInfo.TotalIncome;
    
            //Registro de acumulado de la quincena, basandonos en los acumulado
            var lastIncomeTax = await _unitOfWork.IncomeTaxAccrual.Entities
                .Where(income => income.CollaboratorId == salaryInformation.Collaborator.Id && income.PayrollId == payrollId)
                .FirstOrDefaultAsync(default);

            if (lastIncomeTax is null)
            {
                _logger.LogInformation("No se puedo encontrar el ultimo registro acumulados del colaborador {identification}", collaboratorInformation.IdentificationNumber);
                return;
            }
                                    
            _logger.LogInformation("Calculando inss e ir");

            var (BiweeklyInss, BiweeklyIr) = await _calculatorDeductions.CalculateIr(
                lastIncomeTax?.NumberOfFortnights ?? 24,
                lastIncomeTax?.SalaryEarned       ?? 0.0m,
                lastIncomeTax?.AccumulatedIR      ?? 0.0m,
                GrossSalary,
                default
            );

            //Actualizar datos de deducciones.
            ordinaryPayrollInfo.Ir                   = BiweeklyIr;
            ordinaryPayrollInfo.Inss                 = BiweeklyInss;
            ordinaryPayrollInfo.TotalLegalDeductions = BiweeklyInss + BiweeklyIr;

            lastIncomeTax?.FlagAccumulatedIR = lastIncomeTax?.AccumulatedIR + BiweeklyIr;
            lastIncomeTax?.FlagSalaryEarned =  ordinaryPayrollInfo.TotalIncome - BiweeklyInss;

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
            ordinaryPayrollInfo.NumberOvertime   = totalHours;
            ordinaryPayrollInfo.TotalDeducctions = totalDeductions + BiweeklyIr + BiweeklyInss;

            //Actualizamos su acumulado
            await _unitOfWork.IncomeTaxAccrual.UpdateAsync(lastIncomeTax!);

            //Actualizamos la nomina
            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayrollInfo);    

            //Registro de horas extras
            await _unitOfWork.Incomes.RegisterIncome(new()
            {
                Currency        = Currency.NIO,
                AmountInLocal   = AmountTotalWithHours,
                AmountInDollars = AmountTotalWithHours / 36.6243m,
                CollaboratorId  = salaryInformation.Collaborator.Id,
                IncomeTypeId    = typeIncomeId,
                PayrollId       = payrollId,
                Description     = "Horas extras",                        
            });        
        }

        public async Task ApplyIncomeCommissions(Collaborator collaboratorInformation, Salary salaryInformation, decimal amountComission, Currency currency, Guid payrollId, Guid incomeTypeId)
        {
            var ordinaryPayrollInfo = await _unitOfWork.OrdinaryPayrolls.Entities
                .Include(ord => ord.Payroll)
                .Where(ord => ord.CollaboratorId == collaboratorInformation.Id && ord.PayrollId == payrollId)
                .FirstOrDefaultAsync(default);
                
            if (ordinaryPayrollInfo is null)
            {
                _logger.LogInformation("No se encontro registro del colaborador en la nomina");
                return;
            }

            _logger.LogInformation("Agregando Ingreso de comisiones");

            var lastIncomeTax = await _unitOfWork.IncomeTaxAccrual.Entities
                .Where(income => income.CollaboratorId == collaboratorInformation.Id && income.PayrollId == payrollId)
                .FirstOrDefaultAsync(default);

            if (lastIncomeTax is null)
            {
                _logger.LogInformation("No se puedo encontrar el ultimo registro acumulados del colaborador");
                return;
            }

            int daysWorked = 15;
            DateTime entryDate = salaryInformation.Collaborator.WorkingInformation.EntryDate;
            DateTime payrollStart = salaryInformation.StartDate;

            DateTime payrollEnd = ordinaryPayrollInfo.Payroll.EndDate;

            if (entryDate > payrollStart) daysWorked = (payrollEnd - entryDate).Days + 1;
            else  daysWorked = 15;

            if (daysWorked < 0) daysWorked = 0;
            if (daysWorked > 15) daysWorked = 15;

            decimal TotalIncome = ordinaryPayrollInfo.Antique + ordinaryPayrollInfo.Overtime + ordinaryPayrollInfo.Bonus + ordinaryPayrollInfo.BiweeklySalary;
            
            var comission = amountComission;

            if (currency == Currency.USD)
            {
                comission = amountComission * 36.6243m;
            }

            TotalIncome += comission;         
            ordinaryPayrollInfo.TotalIncome = TotalIncome;
            ordinaryPayrollInfo.Commissions = comission;

            var (BiweeklyInss, BiweeklyIr) = await _calculatorDeductions.CalculateIr(
                lastIncomeTax.NumberOfFortnights,
                lastIncomeTax?.SalaryEarned       ?? 0.0m,
                lastIncomeTax?.AccumulatedIR      ?? 0.0m,
                TotalIncome,
                default
            );

            lastIncomeTax?.FlagAccumulatedIR = lastIncomeTax?.AccumulatedIR + BiweeklyIr;
            lastIncomeTax?.FlagSalaryEarned  = TotalIncome - BiweeklyInss;

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
            ordinaryPayrollInfo.TotalToPay = total;

            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayrollInfo);

            await _unitOfWork.Incomes.RegisterIncome(new()
            {
                CollaboratorId  =  collaboratorInformation.Id,
                AmountInDollars = amountComission / 3.6246m,
                AmountInLocal   = amountComission,
                Currency        = currency,
                IncomeTypeId    = incomeTypeId,
                Description     = "Ingreso comisiones",
                PayrollId       = payrollId,
            });
        }
    }
    
    #pragma warning restore CA1873
}