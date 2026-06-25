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
    public class IncomeServices(IUnitOfWork _unitOfWork,ICalculatorDeductions _calculatorDeductions, ILogger<CalculatorDeductions> _logger) : IIncomeServices
    {
        public async Task<bool> ApplyMedicalSubsidyToPregnantWomen()
        {
            //Logica registro de subsidio de embarazada.

            return true;
        }

        public async Task<bool> ApplyMedicalSubsidy(Collaborator collaboratorInformation, Salary salaryInformation,Payroll period, RegisterSubsidyCommmand data)
        {
            _logger.LogInformation("🚩Iniciando proceso de subsidio para el colaborador: {identification}", collaboratorInformation.IdentificationNumber);

            //Control de pago de viaticos.
            var travelExpensePayments = await _unitOfWork.RecordsTravelExpensePayments.Entities
                .Where(travel => travel.CollaboratorId == collaboratorInformation.Id)
                .Where(travel => travel.PayrollId == period.Id)
                .FirstOrDefaultAsync(default);

            if (travelExpensePayments is null)
            {
                _logger.LogInformation("El control de pago de viaticos del colaborador con cedula {identification} no fue encontrado", collaboratorInformation.IdentificationNumber);
                return false;
            }

            //Control de acumulados del colaborador.
            var taxIncome = await _unitOfWork.IncomeTaxAccrual.Entities
                .Where(tax => tax.PayrollId == period.Id)
                .Where(tax => tax.CollaboratorId == collaboratorInformation.Id)
                .FirstOrDefaultAsync(default);

            if (taxIncome is null)
            {
                _logger.LogInformation("El control de acumulado del colaborador con cedula {identification} no fue encontrado", collaboratorInformation.IdentificationNumber);
                return false;
            }

            decimal monthlySalary   = salaryInformation.AmountInLocal;
            decimal dailySalary     = monthlySalary / 30;

            var informationPayroll = await _unitOfWork.OrdinaryPayrolls.Entities
                .Include(ord => ord.Payroll)
                .Where(ord => ord.PayrollId == period.Id)
                .Where(ord => ord.CollaboratorId == collaboratorInformation.Id)
                .FirstOrDefaultAsync(default);

            if (informationPayroll is null)
            {
                _logger.LogInformation("No se la información contable de la nomina, de este colaborador: {identification}", collaboratorInformation.IdentificationNumber);
                return false;
            }

            #region Iniciar proceso de calculo de dias de subsidio dentro de la nomina

            DateOnly payrollStartDate = period.StartDate;
            DateOnly payrollEndDate   = period.EndDate;

            DateOnly subsidyStartDate = DateOnly.FromDateTime(data.StartDate.Date);
            DateOnly subsidyEndDate   = DateOnly.FromDateTime(data.EndDate.Date);

            DateOnly effectiveStart = subsidyStartDate;
            DateOnly effectiveEnd = subsidyEndDate > payrollEndDate
                ? payrollEndDate
                : subsidyEndDate;
                
            if (effectiveEnd < effectiveStart)
            {
                _logger.LogInformation("La fecha final del subsidio es inválida.");
                return false;
            }

            //Calcular los dias con subsidios
            int subsidizedDays = effectiveEnd.DayNumber - effectiveStart.DayNumber + 1;
            int daysWithoutSubsidy = 15 - subsidizedDays;

            // A: 
            decimal proportionalSalaryWithoutSubsidy = dailySalary * daysWithoutSubsidy;
            proportionalSalaryWithoutSubsidy += informationPayroll.Antique + informationPayroll.Overtime + informationPayroll.Bonus + informationPayroll.Commissions;

            // B:
            decimal proportionalSalaryWithSubsidy = dailySalary * subsidizedDays;

            decimal inssWithoutSubsidy = await _calculatorDeductions.CalculateInss(proportionalSalaryWithoutSubsidy, default);
            decimal GrossSalaryWithoutSubsidy = proportionalSalaryWithoutSubsidy - inssWithoutSubsidy;

            //Sacar el 40% del pago de salario a los dias subsidiados
            decimal GrossSalaryWithSubsidy = proportionalSalaryWithSubsidy * 0.4m;

            //Sacar el 40% a los dias no subsidiado.

            decimal TotalGrossSalary = GrossSalaryWithSubsidy + GrossSalaryWithoutSubsidy;

            //Aplicamos inss.
            var (BiweeklyInss, BiweeklyIr) = await _calculatorDeductions.CalculateIr(
                taxIncome?.NumberOfFortnights   ?? 24,
                taxIncome?.SalaryEarned         ?? 0,
                taxIncome?.AccumulatedIR        ?? 0,
                TotalGrossSalary,
                true
            );

            informationPayroll.Inss = inssWithoutSubsidy;
            informationPayroll.Ir = BiweeklyIr;

            informationPayroll.TotalLegalDeductions = inssWithoutSubsidy + BiweeklyIr;

            taxIncome?.FlagSalaryEarned += TotalGrossSalary;
            taxIncome?.FlagAccumulatedIR += BiweeklyIr;
            
            informationPayroll.TotalLegalDeductions = inssWithoutSubsidy + BiweeklyIr;


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

            _logger.LogInformation("✅Subsidio aplicado con exito.");

            _logger.LogInformation("🚩Empezando proceso para disminución de viaticos por dias no laborados por subsidios");

            var assignedTravelExpensive = await _unitOfWork.AssignedTravelExpenses.Entities
                .Where(assig => assig.CollaboratorId == collaboratorInformation.Id)
                .Where(assig => assig.EndDate == null)
                .Include(assig => assig.TypeIncome)
                .ToListAsync(default);

            decimal transport = 0.0m;
            decimal feeding   = 0.0m;
            decimal lodging   = 0.0m;

            foreach (var assig in assignedTravelExpensive)
            {
                if (assig.TypeIncome.IncomeCode == "ALW_MEAL")
                {
                    feeding = assig.AmountInLocalCurrency;
                    continue;
                }
                if (assig.TypeIncome.IncomeCode == "ALW_TRANSPORT")
                {
                    transport = assig.AmountInLocalCurrency;
                    continue;
                }
                if (assig.TypeIncome.IncomeCode == "ALW_HOUSING")
                {
                    lodging = assig.AmountInLocalCurrency;
                    continue;
                }
            }

            int totalDays = subsidyEndDate.DayNumber - subsidyStartDate.DayNumber + 1;
            int sundays = 0;

            for (DateOnly date = subsidyStartDate; date <= subsidyEndDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    sundays++;
                }
            }

            totalDays -= sundays;

            if (!collaboratorInformation.DoesWorkSaturdays)
            {
                int saturdays = 0;

                for (DateOnly date = subsidyStartDate; date <= subsidyEndDate; date = date.AddDays(1))
                {
                    if (date.DayOfWeek == DayOfWeek.Saturday)
                    {
                        saturdays++;
                    }
                }

                totalDays -= saturdays;
            }

            totalDays = Math.Max(totalDays, 0);


            decimal totalDeductionTravelExpensive = (transport + feeding + lodging) * totalDays;

            informationPayroll.TotalTravelExpenses -= totalDeductionTravelExpensive;

            informationPayroll.TotalToPay = informationPayroll.TotalIncome - informationPayroll.TotalLegalDeductions - totalDeductions + informationPayroll.TotalTravelExpenses;
        
            travelExpensePayments?.PaidDays = totalDays;
            travelExpensePayments?.Lodging = lodging * totalDays;
            travelExpensePayments?.Transport = transport * totalDays;
            travelExpensePayments?.Feeding = feeding * totalDays;

            await _unitOfWork.RecordsTravelExpensePayments.UpdateAsync(travelExpensePayments!);

            //Actualizar información de la nomina en progreso.
            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(informationPayroll);

            await _unitOfWork.Subsidies.CreateSubsidy(new ()
            {
                AmountDays      = subsidizedDays,
                CollaboratorId  = collaboratorInformation.Id,
                PayrollId       = data.PayrollId,
                StartDate       = data.StartDate,
                EndDate         = data.EndDate,
                Observations    = data.Observations,
                ReferenceNumber = data.ReferenceNumber,
                TypeSubsidyId   = data.TypeSubsidyId,
                Percentage      = 40,
            });

            _logger.LogInformation("✅Deducción de viaticos realizados correctamente.");

            return true;
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
            
            int daysWorked = 15;
            DateOnly entryDate = salaryInformation.Collaborator.WorkingInformation.EntryDate;
            DateOnly payrollStart = ordinaryPayrollInfo.Payroll.StartDate;
            DateOnly payrollEnd = ordinaryPayrollInfo.Payroll.EndDate;

            if (entryDate > payrollStart) daysWorked = payrollEnd.DayNumber - entryDate.DayNumber + 1;
            else  daysWorked = 15;

            if (daysWorked < 0) daysWorked = 0;
            if (daysWorked > 15) daysWorked = 15;

            decimal salaryDaily = salaryInformation.AmountInLocal / 30;
            decimal salaryProportional = salaryDaily * daysWorked;

            decimal TotalIncome = ordinaryPayrollInfo.Antique + ordinaryPayrollInfo.Overtime + ordinaryPayrollInfo.Commissions + salaryProportional;

            var bonus = amountBonus;

            if (currency == Currency.USD)
            {
                bonus = amountBonus * 36.6243m;
            }

            bonus += ordinaryPayrollInfo.Vacations;
            TotalIncome += ordinaryPayrollInfo.Vacations;
            
            ordinaryPayrollInfo.TotalIncome = TotalIncome;
            ordinaryPayrollInfo.Bonus = bonus;

            var (BiweeklyInss, BiweeklyIr) = await _calculatorDeductions.CalculateIr(
                lastIncomeTax.NumberOfFortnights,
                lastIncomeTax?.SalaryEarned       ?? 0.0m,
                lastIncomeTax?.AccumulatedIR      ?? 0.0m,
                TotalIncome,
                false,
                bonus
            );

            TotalIncome += bonus;
            TotalIncome += ordinaryPayrollInfo.Vacations;

            lastIncomeTax?.FlagAccumulatedIR = lastIncomeTax?.AccumulatedIR + BiweeklyIr;
            lastIncomeTax?.FlagSalaryEarned  = lastIncomeTax?.SalaryEarned + (TotalIncome - BiweeklyInss);

            //Actualizar datos de deducciones.
            ordinaryPayrollInfo.Ir                   += BiweeklyIr;
            ordinaryPayrollInfo.Inss                 += BiweeklyInss;
            ordinaryPayrollInfo.TotalLegalDeductions = ordinaryPayrollInfo.Ir + ordinaryPayrollInfo.Inss;

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

            ordinaryPayrollInfo.TotalDeducctions = BiweeklyInss + BiweeklyIr + totalDeductions;

            decimal total = ordinaryPayrollInfo.TotalIncome - ordinaryPayrollInfo.TotalDeducctions + ordinaryPayrollInfo.TotalTravelExpenses;

            ordinaryPayrollInfo.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);
            ordinaryPayrollInfo.TotalToPay = total;
    
            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayrollInfo);
            await _unitOfWork.IncomeTaxAccrual.UpdateAsync(lastIncomeTax!);

            await _unitOfWork.Incomes.RegisterIncome(new()
            {
                CollaboratorId  =  collaboratorInformation.Id,
                AmountInDollars = amountBonus / 3.6246m,
                AmountInLocal   = amountBonus,
                Currency        = currency,
                IncomeTypeId    = incomeTypeId,
                Description     = "Ingreso de bonos",
                PayrollId       = payrollId,
            });
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
            DateOnly entryDate      = salaryInformation.Collaborator.WorkingInformation.EntryDate;
            DateOnly payrollStart   = ordinaryPayrollInfo.Payroll.StartDate;
            DateOnly payrollEnd     = ordinaryPayrollInfo.Payroll.EndDate;

            daysWorked = entryDate > payrollStart
                ? payrollEnd.DayNumber - entryDate.DayNumber + 1
                : 15;

            if (daysWorked < 0) daysWorked = 0;
            if (daysWorked > 15) daysWorked = 15;

            decimal ProportionalBiweeklySalary = DailySalary * daysWorked;
            decimal AmountTotalWithHours       = HourlyWage * totalHours * 2;

            ordinaryPayrollInfo.Overtime        = AmountTotalWithHours;                    
            ordinaryPayrollInfo.NumberOvertime  = totalHours;
            ordinaryPayrollInfo.TotalIncome     = ordinaryPayrollInfo.Commissions + AmountTotalWithHours + ProportionalBiweeklySalary + ordinaryPayrollInfo.Antique;

            decimal GrossSalary       = ordinaryPayrollInfo.TotalIncome;
            decimal AdditionalPayment = ordinaryPayrollInfo.Bonus + ordinaryPayrollInfo.Vacations;
    
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
                false,
                AdditionalPayment
            );

            //Actualizar datos de deducciones.
            ordinaryPayrollInfo.Ir                   = BiweeklyIr;
            ordinaryPayrollInfo.Inss                 = BiweeklyInss;
            ordinaryPayrollInfo.TotalLegalDeductions = BiweeklyInss + BiweeklyIr;

            //Actualización de acumulados.
            lastIncomeTax?.FlagAccumulatedIR = lastIncomeTax?.AccumulatedIR + BiweeklyIr;
            lastIncomeTax?.FlagSalaryEarned =  lastIncomeTax?.SalaryEarned + (ordinaryPayrollInfo.TotalIncome - BiweeklyInss);

            //Calculo de comisiones.
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

            ordinaryPayrollInfo.TotalLegalDeductions = BiweeklyInss + BiweeklyIr + totalDeductions;
            ordinaryPayrollInfo.TotalToPay           = ordinaryPayrollInfo.TotalIncome - ordinaryPayrollInfo.TotalDeducctions + ordinaryPayrollInfo.TotalTravelExpenses;
            ordinaryPayrollInfo.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);
                        
            ordinaryPayrollInfo.GrossSalary      = salaryInformation.AmountInLocal / 2;
            ordinaryPayrollInfo.NumberOvertime   = totalHours;

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

            DateOnly entryDate      = salaryInformation.Collaborator.WorkingInformation.EntryDate;
            DateOnly payrollStart   = ordinaryPayrollInfo.Payroll.StartDate;
            DateOnly payrollEnd     = ordinaryPayrollInfo.Payroll.EndDate;

            if (entryDate > payrollStart) daysWorked = payrollEnd.DayNumber - entryDate.DayNumber + 1;
            else  daysWorked = 15;

            if (daysWorked < 0) daysWorked = 0;
            if (daysWorked > 15) daysWorked = 15;

            decimal salaryDaily         = salaryInformation.AmountInLocal / 30;
            decimal salaryProportional  = salaryDaily * daysWorked;

            //No tomamos en cuenta las comisiones.
            decimal TotalIncome         = ordinaryPayrollInfo.Antique + ordinaryPayrollInfo.Overtime + salaryProportional;
            
            var comission = amountComission;

            if (currency == Currency.USD)
            {
                comission = amountComission * 36.6243m;
            }

            //Actualizamos el total de ingresos
            TotalIncome += comission;
            ordinaryPayrollInfo.TotalIncome = TotalIncome;
            ordinaryPayrollInfo.Commissions = comission;


            //Sumar todos los pagos adicionales
            decimal additionalPayment = ordinaryPayrollInfo.Vacations + ordinaryPayrollInfo.Bonus;

            var (BiweeklyInss, BiweeklyIr) = await _calculatorDeductions.CalculateIr(
                lastIncomeTax.NumberOfFortnights,
                lastIncomeTax?.SalaryEarned       ?? 0.0m,
                lastIncomeTax?.AccumulatedIR      ?? 0.0m,
                TotalIncome
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

            ordinaryPayrollInfo.TotalDeducctions = BiweeklyInss + BiweeklyIr + totalDeductions;

            decimal total = ordinaryPayrollInfo.TotalIncome - ordinaryPayrollInfo.TotalDeducctions + ordinaryPayrollInfo.TotalTravelExpenses;

            ordinaryPayrollInfo.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);
            ordinaryPayrollInfo.TotalToPay = total;

    
            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayrollInfo);
            await _unitOfWork.IncomeTaxAccrual.UpdateAsync(lastIncomeTax!);

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
    
        //✅Pago de vacaciones.
        public async Task<bool> ApplyVacationPay(Collaborator collaboratorInformation, Salary salaryInformation, Guid payrollId, decimal amountDays)
        {
            var ordinaryPayrollInfo = await _unitOfWork.OrdinaryPayrolls.Entities
                .Where(ord => ord.PayrollId == payrollId)
                .Where(ord => ord.CollaboratorId == collaboratorInformation.Id)
                .Include(ord => ord.Payroll)
                .FirstOrDefaultAsync(default);

            if (ordinaryPayrollInfo is null)
            {
                _logger.LogInformation("No se encontro la información de nomina de este colaborador: {identification}", collaboratorInformation.IdentificationNumber);   
                return false;
            }

            var lastIncomeTax = await _unitOfWork.IncomeTaxAccrual.Entities
                .Where(income => income.CollaboratorId == collaboratorInformation.Id && income.PayrollId == payrollId)
                .FirstOrDefaultAsync(default);

            if (lastIncomeTax is null)
            {
                _logger.LogInformation("No se puedo encontrar el ultimo registro acumulados del colaborador");
                return false;
            }
            
            int daysWorked = 15;
            DateOnly entryDate = salaryInformation.Collaborator.WorkingInformation.EntryDate;
            DateOnly payrollStart = ordinaryPayrollInfo.Payroll.StartDate;
            DateOnly payrollEnd = ordinaryPayrollInfo.Payroll.EndDate;

            if (entryDate > payrollStart) daysWorked = payrollEnd.DayNumber - entryDate.DayNumber + 1;
            else  daysWorked = 15;

            if (daysWorked < 0) daysWorked = 0;
            if (daysWorked > 15) daysWorked = 15;

            decimal salaryDaily = salaryInformation.AmountInLocal / 30;
            decimal salaryProportional = salaryDaily * daysWorked;

            decimal TotalIncome = ordinaryPayrollInfo.Antique + ordinaryPayrollInfo.Overtime + ordinaryPayrollInfo.Commissions + salaryProportional;

            ordinaryPayrollInfo.TotalIncome = TotalIncome;

            //Calculamos el total de pago de vacaciones, cualquier pago adiciona que tenga
            decimal amountVacation      = amountDays * salaryDaily;
            decimal additionalPayment   = ordinaryPayrollInfo.Bonus + amountVacation;

            //Realizamos el calculo de inss e ir, con pago adicional.
            var (BiweeklyInss, BiweeklyIr) = await _calculatorDeductions.CalculateIr(
                lastIncomeTax.NumberOfFortnights,
                lastIncomeTax?.SalaryEarned       ?? 0.0m,
                lastIncomeTax?.AccumulatedIR      ?? 0.0m,
                TotalIncome,
                false,
                additionalPayment
            );

            //Sumamos pagos adiciones que tenga, pago de vacaciones y bonos
            TotalIncome += amountVacation;
            TotalIncome += ordinaryPayrollInfo.Bonus;

            //Actualizamos el acumulado para la siguiente quincena
            if (lastIncomeTax?.NumberOfFortnights == 1)
            {
                lastIncomeTax?.FlagAccumulatedIR = 0.0m;
                lastIncomeTax?.FlagSalaryEarned  = 0.0m;

                //Aqui va el registro para el acumulado final de año.
            }
            else
            {
                lastIncomeTax?.FlagAccumulatedIR = lastIncomeTax?.AccumulatedIR + BiweeklyIr;
                lastIncomeTax?.FlagSalaryEarned  = lastIncomeTax?.SalaryEarned  + (TotalIncome - BiweeklyInss);   
            }

            //Actualizar datos de deducciones.  
            ordinaryPayrollInfo.Ir                   = BiweeklyIr;
            ordinaryPayrollInfo.Inss                 = BiweeklyInss;

            //Actualizamos el total de ingresos y deducciones de ley.
            ordinaryPayrollInfo.TotalIncome          = TotalIncome;
            ordinaryPayrollInfo.TotalLegalDeductions = ordinaryPayrollInfo.Ir + ordinaryPayrollInfo.Inss;
            
            //Sumamos el total de deducciones
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

            ordinaryPayrollInfo.TotalDeducctions = BiweeklyInss + BiweeklyIr + totalDeductions;

            //Actualizamos el total a pagar en esta quincena actual.
            ordinaryPayrollInfo.TotalToPay       = ordinaryPayrollInfo.TotalIncome - ordinaryPayrollInfo.TotalDeducctions + ordinaryPayrollInfo.TotalTravelExpenses;

            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayrollInfo);
            await _unitOfWork.IncomeTaxAccrual.UpdateAsync(lastIncomeTax!);

            _logger.LogInformation("✅Pago de vacaciones procesado con exito.");
            return true;
        }
    }
}