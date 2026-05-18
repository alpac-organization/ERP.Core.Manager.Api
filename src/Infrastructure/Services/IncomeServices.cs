using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;


namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    #pragma warning disable CA1873

    public class IncomeServices(IUnitOfWork _unitOfWork,ICalculatorDeductions _calculatorDeductions, ILogger<CalculatorDeductions> _logger) : IIncomeServices
    {
        public async Task ApplyIncomeOvertime(Collaborator collaboratorInformation, Salary salaryInformation, decimal totalHours, Guid payrollId, Guid typeIncomeId)
        {
            var ordinaryPayrollInfo = await _unitOfWork.OrdinaryPayrolls.Entities
                .Include(ord => ord.Payroll)
                .Where(ord => ord.PayrollId == payrollId)
                .Where(ord => ord.CollaboratorId ==collaboratorInformation.Id)
                .FirstOrDefaultAsync(default);
                
            if (ordinaryPayrollInfo is null)
            {
                _logger.LogInformation("No se encontro registro del colaborador con identificación {identification} en la nomina", collaboratorInformation.IdentificationNumber);
                return;
            }

            decimal DailySalary = salaryInformation.AmountInLocal / 30;
            decimal HourlyWage = DailySalary / 8;
                        
            int daysWorked = 15;
            DateTime entryDate = salaryInformation.Collaborator.WorkingInformation.EntryDate;
            DateTime payrollStart = salaryInformation.StartDate;

            DateTime payrollEnd = ordinaryPayrollInfo.Payroll.EndDate ?? payrollStart.AddDays(14);

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

            var lastIncomeTax = await _unitOfWork.IncomeTaxAccrual.Entities
                .Where(income => income.CollaboratorId == salaryInformation.Collaborator.Id && income.PayrollId == payrollId)
                .FirstOrDefaultAsync(default);

            if (lastIncomeTax is null)
            {
                _logger.LogInformation("No se puedo encontrar el ultimo registro acumulados del colaborador {identification}", collaboratorInformation.IdentificationNumber);
                return;
            }

            var lastFortnight = lastIncomeTax?.NumberOfFortnights + 1;

            var TaxInformation = await _unitOfWork.IncomeTaxAccrual.Entities
                .Where(income => income.CollaboratorId == salaryInformation.Collaborator.Id && income.NumberOfFortnights == lastFortnight)
                .OrderByDescending(income => income.CreatedAt)
                .FirstOrDefaultAsync(default);

                        
            _logger.LogInformation("Calculando inss e ir");

            var (BiweeklyInss, BiweeklyIr) = await _calculatorDeductions.CalculateIrToNextProcess(
                TaxInformation?.NumberOfFortnights ?? 24,
                TaxInformation?.SalaryEarned       ?? 0.0m,
                TaxInformation?.AccumulatedIR      ?? 0.0m,
                GrossSalary,
                default
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

        public async Task ApplyIncomeCommissions(Collaborator collaboratorInformation, Salary salaryInformation, decimal amountComission, Guid payrollId, Guid incomeTypeId)
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

            var lastFortnight = lastIncomeTax?.NumberOfFortnights + 1;
            if (lastFortnight is 25) lastFortnight = 24;

            var TaxInformation = await _unitOfWork.IncomeTaxAccrual.Entities
                .Where(income => income.CollaboratorId == salaryInformation.Collaborator.Id && income.NumberOfFortnights == lastFortnight)
                .OrderByDescending(income => income.CreatedAt)
                .FirstOrDefaultAsync(default);
        
            int daysWorked = 15;
            DateTime entryDate = salaryInformation.Collaborator.WorkingInformation.EntryDate;
            DateTime payrollStart = salaryInformation.StartDate;

            DateTime payrollEnd = ordinaryPayrollInfo.Payroll.EndDate ?? payrollStart.AddDays(14);

            if (entryDate > payrollStart) daysWorked = (payrollEnd - entryDate).Days + 1;
            else  daysWorked = 15;

            if (daysWorked < 0) daysWorked = 0;
            if (daysWorked > 15) daysWorked = 15;

            decimal TotalIncome = ordinaryPayrollInfo.Antique + ordinaryPayrollInfo.Overtime + ordinaryPayrollInfo.Bonus + ordinaryPayrollInfo.BiweeklySalary;

            TotalIncome += amountComission;         
            ordinaryPayrollInfo.TotalIncome = TotalIncome;
            ordinaryPayrollInfo.Commissions = amountComission;

            var (BiweeklyInss, BiweeklyIr) = await _calculatorDeductions.CalculateIrToNextProcess(
                lastFortnight ?? 24,
                TaxInformation?.SalaryEarned       ?? 0.0m,
                TaxInformation?.AccumulatedIR      ?? 0.0m,
                TotalIncome,
                default
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
            ordinaryPayrollInfo.TotalToPay = total;

            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayrollInfo);
        }
    }
    
    #pragma warning restore CA1873
}