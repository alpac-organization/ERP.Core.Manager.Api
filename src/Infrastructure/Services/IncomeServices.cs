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
    public class IncomeServices(IUnitOfWork _unitOfWork, ICalculatorDeductions _calculatorDeductions, ILogger<CalculatorDeductions> _logger) : IIncomeServices
    {
        public async Task<bool> ApplyMedicalSubsidyToPregnantWomen(Collaborator collaborator, Payroll period, Salary salary, RegisterSubsidyCommmand subsidyData)
        {
            //Logica registro de subsidio de embarazada.

            _logger.LogInformation("🚩Iniciando proceso de subsidio para el colaborador: {identification}", collaborator.IdentificationNumber);

            decimal monthSalary = salary.AmountInLocal;
            decimal daySalary = monthSalary / 30;

            var taxIncome = await _unitOfWork.IncomeTaxAccrual.Entities
                .Where(tax => tax.PayrollId == period.Id)
                .Where(tax => tax.CollaboratorId == collaborator.Id)
                .FirstOrDefaultAsync(default);


            if (taxIncome is null)
            {
                _logger.LogInformation("El control de acumulado del colaborador con cedula {identification} no fue encontrado", collaborator.IdentificationNumber);
                return false;
            }

            var infPayroll = await _unitOfWork.OrdinaryPayrolls.Entities
                .Include(o => o.Payroll)
                .Where(o => o.PayrollId == period.Id)
                .Where(o => o.CollaboratorId == collaborator.Id)
                .FirstOrDefaultAsync(default);


            if (infPayroll is null)
            {
                _logger.LogInformation("El control de acumulado del colaborador con cedula {identification} no fue encontrado", collaborator.IdentificationNumber);
                return false;
            }

            var travelExpensePayments = await _unitOfWork.RecordsTravelExpensePayments.Entities
                .Where(travel => travel.CollaboratorId == collaborator.Id)
                .Where(travel => travel.PayrollId == period.Id)
                .FirstOrDefaultAsync(default);

            DateOnly payrollStartDate = period.StartDate;
            DateOnly payrollEndDate = period.EndDate;
            DateOnly entryDate = collaborator.WorkingInformation.EntryDate;

            int maximumWorkedDays = 15;
            if (entryDate > payrollStartDate)
            {
                maximumWorkedDays = payrollEndDate.DayNumber - entryDate.DayNumber + 1;
            }
            if (maximumWorkedDays < 0) maximumWorkedDays = 0;
            if (maximumWorkedDays > 15) maximumWorkedDays = 15;

            DateOnly subsidyStartDate = DateOnly.FromDateTime(subsidyData.StartDate.Date);
            DateOnly subsidyEndDate = DateOnly.FromDateTime(subsidyData.EndDate.Date);

            DateOnly exactSubsidyStartDate = subsidyStartDate > payrollEndDate ? payrollStartDate : subsidyStartDate < payrollStartDate ? payrollStartDate : subsidyStartDate;

            DateOnly exactSubsidyEndDate = subsidyEndDate > payrollEndDate ? payrollEndDate : subsidyEndDate < payrollStartDate ? payrollEndDate : subsidyEndDate;

            if (exactSubsidyEndDate < exactSubsidyStartDate || exactSubsidyStartDate > exactSubsidyEndDate)
            {
                _logger.LogInformation("Las fechas del subsidio no coinciden con la nomina actual");
                return false;
            }

            int subsidyDays = exactSubsidyEndDate.DayNumber - exactSubsidyStartDate.DayNumber + 1;
            int daysWithoutSubsidy = Math.Max(maximumWorkedDays - subsidyDays, 0);

            var variableIncomeForWorkedDays = infPayroll.Antique
                                              + infPayroll.Overtime
                                              + infPayroll.Vacations
                                              + infPayroll.Commissions;
            //se suma el salario proporcional de dias laborados + otros ingresos.
            decimal totalGrossIncomeInThisFortnight = (daySalary * daysWithoutSubsidy)
                                                       + variableIncomeForWorkedDays;


            decimal inssWithoutSubsidy = await _calculatorDeductions.CalculateInss(totalGrossIncomeInThisFortnight, default);
            decimal taxableBaseWithoutSubsidy = totalGrossIncomeInThisFortnight - inssWithoutSubsidy;//base IR

            decimal proportionalSalaryWithSubsidy = subsidyDays * daySalary;

            decimal companySubsidyContribution = proportionalSalaryWithSubsidy * 0.4m;
            infPayroll.TotalIncome = totalGrossIncomeInThisFortnight
                                     + infPayroll.Bonus
                                     + companySubsidyContribution;

            int NumberOfFortnight = taxIncome?.NumberOfFortnights ?? 24;
            decimal SalaryEarned = taxIncome?.SalaryEarned ?? 0;
            decimal accumulatedIR = taxIncome?.AccumulatedIR ?? 0;

            var (BiweeklyInss, BiweeklyIr) = await _calculatorDeductions.CalculateIr(NumberOfFortnight, SalaryEarned, accumulatedIR, taxableBaseWithoutSubsidy, true, infPayroll.Bonus);

            infPayroll.Inss = inssWithoutSubsidy + BiweeklyInss;
            infPayroll.Ir = BiweeklyIr;
            infPayroll.TotalLegalDeductions = inssWithoutSubsidy + BiweeklyInss + BiweeklyIr;

            decimal netBonus = infPayroll.Bonus - (infPayroll.Bonus * 0.07m);

            taxIncome?.FlagSalaryEarned = (taxIncome?.SalaryEarned ?? 0)
                                          + taxableBaseWithoutSubsidy + netBonus;

            taxIncome?.FlagAccumulatedIR = (taxIncome?.AccumulatedIR ?? 0)
                                          + BiweeklyIr;


            var deductions = JsonSerializer.Deserialize<DeductionsAdditionalData>(
                infPayroll.DeductionsAdditionalData ?? "{}"
            ) ?? new DeductionsAdditionalData();

            if (daysWithoutSubsidy == 0)
            {
                deductions.Absences = 0;
                deductions.LateArrivals = 0;
                deductions.Sanction = 0;
            }

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


            infPayroll.TotalDeducctions = infPayroll.TotalLegalDeductions + totalDeductions;
            infPayroll.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);

            _logger.LogInformation("✅ Subsidio aplicado de manera correcta");

            _logger.LogInformation("🚩Empezando proceso para disminución de viaticos por dias no laborados por subsidios");

            var assignedTravelExpensive = await _unitOfWork.AssignedTravelExpenses.Entities
            .Where(assig => assig.CollaboratorId == collaborator.Id)
            .Where(assig => assig.EndDate == null)
            .Include(assig => assig.TypeIncome)
            .ToListAsync(default);

            decimal transport = 0.0m;
            decimal feeding = 0.0m;
            decimal lodging = 0.0m;
            foreach (var assig in assignedTravelExpensive)
            {
                if (assig.TypeIncome.IncomeCode == "ALW_MEAL")
                {
                    feeding += assig.AmountInLocalCurrency;
                    continue;
                }
                if (assig.TypeIncome.IncomeCode == "ALW_TRANSPORT")
                {
                    transport += assig.AmountInLocalCurrency;
                    continue;
                }
                if (assig.TypeIncome.IncomeCode == "ALW_HOUSING")
                {
                    lodging += assig.AmountInLocalCurrency;
                    continue;
                }
            }

            int totalDaysToDiscount = subsidyDays;
            int sundays = 0;

            for (DateOnly date = exactSubsidyStartDate; date <= exactSubsidyEndDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Sunday) sundays++;
            }

            totalDaysToDiscount -= sundays;

            if (!collaborator.DoesWorkSaturdays)
            {
                int saturdays = 0;
                for (DateOnly date = exactSubsidyStartDate; date <= exactSubsidyEndDate; date = date.AddDays(1))
                {
                    if (date.DayOfWeek == DayOfWeek.Saturday) saturdays++;
                }
                totalDaysToDiscount -= saturdays;
            }

            totalDaysToDiscount = Math.Max(totalDaysToDiscount, 0);

            decimal totalDeductionTravelExpensive = (transport + feeding + lodging) * totalDaysToDiscount;
            infPayroll.TotalTravelExpenses -= totalDeductionTravelExpensive;

            if (travelExpensePayments != null)
            {
                travelExpensePayments.PaidDays = Math.Max(travelExpensePayments.PaidDays - totalDaysToDiscount, 0);
                travelExpensePayments.Lodging = lodging * travelExpensePayments.PaidDays;
                travelExpensePayments.Transport = transport * travelExpensePayments.PaidDays;
                travelExpensePayments.Feeding = feeding * travelExpensePayments.PaidDays;


                await _unitOfWork.RecordsTravelExpensePayments.UpdateAsync(travelExpensePayments);
            }

            infPayroll.TotalToPay = infPayroll.TotalIncome - infPayroll.TotalDeducctions + infPayroll.TotalTravelExpenses;

            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(infPayroll);
            await _unitOfWork.IncomeTaxAccrual.UpdateAsync(taxIncome!);

            await _unitOfWork.Subsidies.CreateSubsidy(new()
            {
                AmountDays = subsidyDays,
                CollaboratorId = collaborator.Id,
                PayrollId = subsidyData.PayrollId,
                StartDate = subsidyData.StartDate,
                EndDate = subsidyData.EndDate,
                Observations = subsidyData.Observations,
                ReferenceNumber = subsidyData.ReferenceNumber,
                TypeSubsidyId = subsidyData.TypeSubsidyId,
                Percentage = 40,
            });

            _logger.LogInformation("✅ Subsidio maternal calculado y aplicado correctamente.");
            return true;
        }
        public async Task<bool> ApplyMedicalSubsidy(Collaborator collaboratorInformation, Salary salaryInformation, Payroll period, RegisterSubsidyCommmand data)
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

            decimal monthlySalary = salaryInformation.AmountInLocal;
            decimal dailySalary = monthlySalary / 30;

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
            DateOnly payrollEndDate = period.EndDate;

            DateOnly subsidyStartDate = DateOnly.FromDateTime(data.StartDate.Date);
            DateOnly subsidyEndDate = DateOnly.FromDateTime(data.EndDate.Date);

            DateOnly effectiveStart = subsidyStartDate > payrollEndDate ? payrollStartDate : subsidyStartDate < payrollStartDate ? payrollStartDate : subsidyStartDate;

            DateOnly effectiveEnd = subsidyEndDate > payrollEndDate ? payrollEndDate : subsidyEndDate < payrollStartDate ? payrollEndDate : subsidyEndDate;

            if (effectiveEnd < effectiveStart || effectiveStart > effectiveEnd)
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
                taxIncome?.NumberOfFortnights ?? 24,
                taxIncome?.SalaryEarned ?? 0,
                taxIncome?.AccumulatedIR ?? 0,
                TotalGrossSalary,
                true
            );

            informationPayroll.Inss = inssWithoutSubsidy;
            informationPayroll.Ir = BiweeklyIr;

            informationPayroll.TotalLegalDeductions = inssWithoutSubsidy + BiweeklyIr;

            taxIncome?.FlagSalaryEarned += TotalGrossSalary;
            taxIncome?.FlagAccumulatedIR += BiweeklyIr;



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
            decimal feeding = 0.0m;
            decimal lodging = 0.0m;

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

            await _unitOfWork.Subsidies.CreateSubsidy(new()
            {
                AmountDays = subsidizedDays,
                CollaboratorId = collaboratorInformation.Id,
                PayrollId = data.PayrollId,
                StartDate = data.StartDate,
                EndDate = data.EndDate,
                Observations = data.Observations,
                ReferenceNumber = data.ReferenceNumber,
                TypeSubsidyId = data.TypeSubsidyId,
                Percentage = 40,
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
            else daysWorked = 15;

            if (daysWorked < 0) daysWorked = 0;
            if (daysWorked > 15) daysWorked = 15;

            decimal salaryDaily = salaryInformation.AmountInLocal / 30;
            decimal salaryProportional = salaryDaily * daysWorked;

            decimal TotalIncome = ordinaryPayrollInfo.Antique + ordinaryPayrollInfo.Overtime + ordinaryPayrollInfo.Commissions + salaryProportional;

            //Sumamos los pagos adicionales.
            decimal additionalPayment = amountBonus;

            if (currency == Currency.USD)
            {
                additionalPayment = amountBonus * 36.6243m;
            }

            //Agregar las vacaciones como pago adicional
            ordinaryPayrollInfo.TotalIncome = TotalIncome;
            ordinaryPayrollInfo.Bonus = additionalPayment;

            //agregamos la cantidad de dinero de los pagos de vacaciones como pago adicional
            additionalPayment += ordinaryPayrollInfo.Vacations;

            //Calculo del ir e inss de los pagos adicionales y total de ingresos en los bonos.
            var (BiweeklyInss, BiweeklyIr) = await _calculatorDeductions.CalculateIr(
                lastIncomeTax.NumberOfFortnights,
                lastIncomeTax?.SalaryEarned ?? 0.0m,
                lastIncomeTax?.AccumulatedIR ?? 0.0m,
                TotalIncome,
                false,
                additionalPayment
            );

            TotalIncome += ordinaryPayrollInfo.Bonus;
            TotalIncome += ordinaryPayrollInfo.Vacations;

            //Actualizamos los acumulados.
            lastIncomeTax?.FlagAccumulatedIR = lastIncomeTax?.AccumulatedIR + BiweeklyIr;
            lastIncomeTax?.FlagSalaryEarned = lastIncomeTax?.SalaryEarned + (TotalIncome - BiweeklyInss);

            //Actualizar datos de deducciones.
            ordinaryPayrollInfo.Ir += BiweeklyIr;
            ordinaryPayrollInfo.Inss += BiweeklyInss;
            ordinaryPayrollInfo.TotalLegalDeductions = ordinaryPayrollInfo.Ir + ordinaryPayrollInfo.Inss;
            ordinaryPayrollInfo.TotalIncome = TotalIncome;

            //Suma de deducciones totales del colaborador
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

            //Suma total de todas las deducciones.
            ordinaryPayrollInfo.TotalDeducctions = BiweeklyInss + BiweeklyIr + totalDeductions;
            ordinaryPayrollInfo.TotalToPay = ordinaryPayrollInfo.TotalIncome - ordinaryPayrollInfo.TotalDeducctions + ordinaryPayrollInfo.TotalTravelExpenses;

            ordinaryPayrollInfo.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);
            ordinaryPayrollInfo.TotalToPay = ordinaryPayrollInfo.TotalIncome - ordinaryPayrollInfo.TotalDeducctions + ordinaryPayrollInfo.TotalTravelExpenses;

            //Actualización de información
            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayrollInfo);
            await _unitOfWork.IncomeTaxAccrual.UpdateAsync(lastIncomeTax!);

            //Registro del ingreso.
            await _unitOfWork.Incomes.RegisterIncome(new()
            {
                CollaboratorId = collaboratorInformation.Id,
                AmountInDollars = amountBonus / 3.6246m,
                AmountInLocal = amountBonus,
                Currency = currency,
                IncomeTypeId = incomeTypeId,
                Description = "Ingreso de bonos",
                PayrollId = payrollId,
            });
        }

        //✅ Aplicar calculo de horas extras.
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
            DateOnly entryDate = salaryInformation.Collaborator.WorkingInformation.EntryDate;
            DateOnly payrollStart = ordinaryPayrollInfo.Payroll.StartDate;
            DateOnly payrollEnd = ordinaryPayrollInfo.Payroll.EndDate;

            daysWorked = entryDate > payrollStart
                ? payrollEnd.DayNumber - entryDate.DayNumber + 1
                : 15;

            if (daysWorked < 0) daysWorked = 0;
            if (daysWorked > 15) daysWorked = 15;

            decimal ProportionalBiweeklySalary = DailySalary * daysWorked;
            decimal AmountTotalWithHours = HourlyWage * totalHours * 2;

            ordinaryPayrollInfo.Overtime = AmountTotalWithHours;
            ordinaryPayrollInfo.NumberOvertime = totalHours;
            ordinaryPayrollInfo.TotalIncome = ordinaryPayrollInfo.Commissions + AmountTotalWithHours + ProportionalBiweeklySalary + ordinaryPayrollInfo.Antique;

            decimal GrossSalary = ordinaryPayrollInfo.TotalIncome;
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
                lastIncomeTax?.SalaryEarned ?? 0.0m,
                lastIncomeTax?.AccumulatedIR ?? 0.0m,
                GrossSalary,
                false,
                AdditionalPayment
            );

            //Actualizamos el total de ingresos.
            ordinaryPayrollInfo.TotalIncome += ordinaryPayrollInfo.Vacations + ordinaryPayrollInfo.Bonus;

            //Actualizar datos de deducciones.
            ordinaryPayrollInfo.Ir = BiweeklyIr;
            ordinaryPayrollInfo.Inss = BiweeklyInss;
            ordinaryPayrollInfo.TotalLegalDeductions = BiweeklyInss + BiweeklyIr;

            //Actualización de acumulados.
            lastIncomeTax?.FlagAccumulatedIR = lastIncomeTax?.AccumulatedIR + BiweeklyIr;
            lastIncomeTax?.FlagSalaryEarned = lastIncomeTax?.SalaryEarned + (ordinaryPayrollInfo.TotalIncome - BiweeklyInss);

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
            ordinaryPayrollInfo.TotalToPay = ordinaryPayrollInfo.TotalIncome - ordinaryPayrollInfo.TotalDeducctions + ordinaryPayrollInfo.TotalTravelExpenses;
            ordinaryPayrollInfo.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);

            ordinaryPayrollInfo.GrossSalary = salaryInformation.AmountInLocal / 2;
            ordinaryPayrollInfo.NumberOvertime = totalHours;

            //Actualizamos su acumulado
            await _unitOfWork.IncomeTaxAccrual.UpdateAsync(lastIncomeTax!);

            //Actualizamos la nomina
            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayrollInfo);

            //Registro de horas extras
            await _unitOfWork.Incomes.RegisterIncome(new()
            {
                Currency = Currency.NIO,
                AmountInLocal = AmountTotalWithHours,
                AmountInDollars = AmountTotalWithHours / 36.6243m,
                CollaboratorId = salaryInformation.Collaborator.Id,
                IncomeTypeId = typeIncomeId,
                PayrollId = payrollId,
                Description = "Horas extras",
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

            DateOnly entryDate = salaryInformation.Collaborator.WorkingInformation.EntryDate;
            DateOnly payrollStart = ordinaryPayrollInfo.Payroll.StartDate;
            DateOnly payrollEnd = ordinaryPayrollInfo.Payroll.EndDate;

            if (entryDate > payrollStart) daysWorked = payrollEnd.DayNumber - entryDate.DayNumber + 1;
            else daysWorked = 15;

            if (daysWorked < 0) daysWorked = 0;
            if (daysWorked > 15) daysWorked = 15;

            decimal salaryDaily = salaryInformation.AmountInLocal / 30;
            decimal salaryProportional = salaryDaily * daysWorked;

            //No tomamos en cuenta las comisiones.
            decimal TotalIncome = ordinaryPayrollInfo.Antique + ordinaryPayrollInfo.Overtime + salaryProportional;

            //Realizamos el calculo de la comisión
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

            //Calculamos el ir e inss, de sus ingresos.
            var (BiweeklyInss, BiweeklyIr) = await _calculatorDeductions.CalculateIr(
                lastIncomeTax.NumberOfFortnights,
                lastIncomeTax?.SalaryEarned ?? 0.0m,
                lastIncomeTax?.AccumulatedIR ?? 0.0m,
                TotalIncome,
                false,
                additionalPayment
            );

            //Actualizamos el total de ingresos.
            TotalIncome += ordinaryPayrollInfo.Bonus + ordinaryPayrollInfo.Vacations;
            ordinaryPayrollInfo.TotalIncome = TotalIncome;

            //Actualizamos el acumulado para la siguiente quincena.
            lastIncomeTax?.FlagAccumulatedIR = lastIncomeTax?.AccumulatedIR + BiweeklyIr;
            lastIncomeTax?.FlagSalaryEarned = lastIncomeTax?.SalaryEarned + (TotalIncome - BiweeklyInss);

            //Actualizar datos de deducciones.
            ordinaryPayrollInfo.Ir = BiweeklyIr;
            ordinaryPayrollInfo.Inss = BiweeklyInss;
            ordinaryPayrollInfo.TotalLegalDeductions = BiweeklyInss + BiweeklyIr;

            //Sumamos todas sus deducciones.
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
            ordinaryPayrollInfo.TotalToPay = ordinaryPayrollInfo.TotalIncome - ordinaryPayrollInfo.TotalDeducctions + ordinaryPayrollInfo.TotalTravelExpenses;

            ordinaryPayrollInfo.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);

            //Actualizamos la nomina y las comisiones.
            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayrollInfo);
            await _unitOfWork.IncomeTaxAccrual.UpdateAsync(lastIncomeTax!);

            //Registramos el ingreso de las comisiones.
            await _unitOfWork.Incomes.RegisterIncome(new()
            {
                CollaboratorId = collaboratorInformation.Id,
                AmountInDollars = amountComission / 3.6246m,
                AmountInLocal = amountComission,
                Currency = currency,
                IncomeTypeId = incomeTypeId,
                Description = "Ingreso comisiones",
                PayrollId = payrollId,
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
            else daysWorked = 15;

            if (daysWorked < 0) daysWorked = 0;
            if (daysWorked > 15) daysWorked = 15;

            decimal salaryDaily = salaryInformation.AmountInLocal / 30;
            decimal salaryProportional = salaryDaily * daysWorked;

            decimal TotalIncome = ordinaryPayrollInfo.Antique + ordinaryPayrollInfo.Overtime + ordinaryPayrollInfo.Commissions + salaryProportional;

            ordinaryPayrollInfo.TotalIncome = TotalIncome;

            //Calculamos el total de pago de vacaciones, cualquier pago adiciona que tenga
            decimal amountVacation = amountDays * salaryDaily;
            decimal additionalPayment = ordinaryPayrollInfo.Bonus + amountVacation;

            //Realizamos el calculo de inss e ir, con pago adicional.
            var (BiweeklyInss, BiweeklyIr) = await _calculatorDeductions.CalculateIr(
                lastIncomeTax.NumberOfFortnights,
                lastIncomeTax?.SalaryEarned ?? 0.0m,
                lastIncomeTax?.AccumulatedIR ?? 0.0m,
                TotalIncome,
                false,
                additionalPayment
            );

            //Sumamos pagos adiciones que tenga, pago de vacaciones y bonos
            TotalIncome += amountVacation;
            TotalIncome += ordinaryPayrollInfo.Bonus;

            ordinaryPayrollInfo.Vacations = amountVacation;
            ordinaryPayrollInfo.AmountDaysVacation = amountDays;

            //Actualizamos el acumulado para la siguiente quincena
            if (lastIncomeTax?.NumberOfFortnights == 1)
            {
                lastIncomeTax?.FlagAccumulatedIR = 0.0m;
                lastIncomeTax?.FlagSalaryEarned = 0.0m;

                //Aqui va el registro para el acumulado final de año.
            }
            else
            {
                lastIncomeTax?.FlagAccumulatedIR = lastIncomeTax?.AccumulatedIR + BiweeklyIr;
                lastIncomeTax?.FlagSalaryEarned = lastIncomeTax?.SalaryEarned + (TotalIncome - BiweeklyInss);
            }

            //Actualizar datos de deducciones.  
            ordinaryPayrollInfo.Ir = BiweeklyIr;
            ordinaryPayrollInfo.Inss = BiweeklyInss;

            //Actualizamos el total de ingresos y deducciones de ley.
            ordinaryPayrollInfo.TotalIncome = TotalIncome;
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
            ordinaryPayrollInfo.TotalToPay = ordinaryPayrollInfo.TotalIncome - ordinaryPayrollInfo.TotalDeducctions + ordinaryPayrollInfo.TotalTravelExpenses;

            //Actualizar control de vacaciones
            var vacationControl = await _unitOfWork.Vacations.Entities
                .Where(vac => vac.CollaboratorId == collaboratorInformation.Id)
                .FirstOrDefaultAsync(default);

            if (vacationControl is null)
            {
                _logger.LogInformation("No se encontro el control de vacaciones de este colaborador");
                return false;
            }

            if (vacationControl.AvailableVacations < amountDays)
            {
                _logger.LogInformation("El colaborador con cedula: {identification} no posee la cantidad necesaria de vacaciones, para ser aprobadas", collaboratorInformation.IdentificationNumber);
                return false;
            }

            vacationControl.EnjoyedVacation += amountDays;
            vacationControl.AvailableVacations -= amountDays;

            await _unitOfWork.Vacations.UpdateAsync(vacationControl);
            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayrollInfo);
            await _unitOfWork.IncomeTaxAccrual.UpdateAsync(lastIncomeTax!);

            _logger.LogInformation("✅Pago de vacaciones procesado con exito.");
            return true;
        }
        public async Task ApplyIncomeDepreciation(Collaborator collaboratorInformation, Salary salaryInformation, decimal amountDepreciation, Currency currency, Guid payrollId, Guid incomeTypeId)
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
            _logger.LogInformation("Agregando Ingreso de depreciación de vehículo");

            // se convierte a moneda local si viene en dólares

            var exchangeRate = await _unitOfWork.ValidityDeductions.Entities
                .Where(val => val.Status)
                .Where(val => val.EndDate == null)
                .Where(val => val.Type == TaxType.ExchangeRate)
                .FirstOrDefaultAsync(default);

            decimal finalAmount = amountDepreciation;
            if (currency == Currency.USD)
            {
                finalAmount = amountDepreciation * exchangeRate!.Value;
            }
            await _unitOfWork.Incomes.RegisterIncome(new()
            {
                CollaboratorId = collaboratorInformation.Id,
                AmountInDollars = currency == Currency.USD ? amountDepreciation : (amountDepreciation / exchangeRate!.Value),
                AmountInLocal = finalAmount,
                Currency = currency,
                IncomeTypeId = incomeTypeId,
                Description = "Depreciación actual",
                PayrollId = payrollId,
            });

            _logger.LogInformation("Depreciación registrada exitosamente sin afectar la nómina.");
        }
    }
}