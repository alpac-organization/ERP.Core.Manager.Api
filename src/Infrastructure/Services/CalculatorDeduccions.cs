using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Entities.Catalogs;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
   public class CalculatorDeductions(IUnitOfWork _unitOfWork, ILogger<CalculatorDeductions> _logger) : ICalculatorDeductions
   {
      public (decimal antiquePay, int yearsOfService) CalculateAntique(decimal monthlySalary, DateOnly payrollEnd, DateOnly collaboratorEntryDate)
      {
         _logger.LogInformation("Iniciando Proceso para calcular antiguedad✅");

         int yearsOfService = payrollEnd.Year - collaboratorEntryDate.Year;

         if (collaboratorEntryDate > payrollEnd.AddYears(-yearsOfService))
         {
            yearsOfService--;
         }

         decimal seniorityPercentage = yearsOfService switch
         {
            <= 0 => 0.00m,
            1 => 0.03m,
            2 => 0.05m,
            3 => 0.07m,
            4 => 0.09m,
            5 => 0.10m,
            6 => 0.11m,
            7 => 0.12m,
            8 => 0.13m,
            9 => 0.14m,
            10 => 0.15m,
            11 => 0.155m,
            12 => 0.16m,
            13 => 0.165m,
            14 => 0.17m,
            15 => 0.175m,
            16 => 0.18m,
            17 => 0.185m,
            18 => 0.19m,
            19 => 0.195m,
            _ => 0.20m
         };

         return (seniorityPercentage * monthlySalary, yearsOfService);
      }
      public async Task<decimal> CalculateInss(decimal GrossSalary, CancellationToken cancellationToken)
      {
         //Realizar la consulta a la tabla constantes de deducciones de ley,
         var valuesDeductions = await _unitOfWork.ValidityDeductions.Entities
             .Where(deduction => deduction.Type == TaxType.Inss)
             .Where(deduction => deduction.Status == true)
             .FirstOrDefaultAsync(cancellationToken);

         if (valuesDeductions is null)
         {
            _logger.LogWarning("No se encontró configuración activa de INSS");
            return 0.0m;
         }

         _logger.LogInformation("Iniciando calculo de Inss");

         var result = GrossSalary * valuesDeductions.Value;

         return Math.Round(result, 2, MidpointRounding.AwayFromZero);
      }

      //Este ir se basa en la numero de quincena que se encuentra actualmente el colaborador
      public async Task<IrCalculationResult> CalculateIr(int NFortnight, decimal AccumulatedAccrued, decimal AccumulatedIR, decimal GrossSalary, bool isSudsidy = false, decimal additionalPayment = 0.0m)
      {
         var nextFortnight = NFortnight;
         decimal biweeklyInss = 0.0m;
         decimal inssAdditionalPayment = 0.0m;

         if (!isSudsidy)
         {
            biweeklyInss = await CalculateInss(GrossSalary, default);
         }

         if (additionalPayment > 0)
         {
            inssAdditionalPayment = await CalculateInss(additionalPayment, default);
         }

         //Salario quincenal libre de inss.
         decimal netSalary = GrossSalary - biweeklyInss;
         decimal netAdditionalPayment = additionalPayment - inssAdditionalPayment;

         decimal AnnualSalary = netSalary * nextFortnight;
         decimal AnnualAdditionalPayment = netAdditionalPayment + AnnualSalary;

         decimal totalAnnualSalary = AnnualSalary + AccumulatedAccrued;
         decimal totalAnnualAdditionalPayment = AnnualAdditionalPayment + AccumulatedAccrued;

         decimal AnnualIr;
         decimal AnnualAdditionalIr;

         //Agregar regla del ir
         decimal BaseTax;
         decimal AnnualExpectationIR = 0.0m;
         decimal AnnualExpectationAdditionalIr = 0.0m;

         decimal IrBiweekly = 0.0m;
         decimal IrBiweeklyAdditional = 0.0m;

         if (totalAnnualSalary <= 100000)
         {
            AnnualIr = 0;
         }
         else if (totalAnnualSalary > 100000 && totalAnnualSalary <= 200000)
         {
            BaseTax = 0;
            AnnualIr = ((totalAnnualSalary - 100000) * 0.15m);
            AnnualAdditionalIr = ((totalAnnualAdditionalPayment - 100000) * 0.15m);

            AnnualExpectationIR = AnnualIr + BaseTax;
            AnnualExpectationAdditionalIr = AnnualAdditionalIr + BaseTax;

            if (additionalPayment == 0)
            {
               IrBiweekly = (AnnualExpectationIR - AccumulatedIR) / nextFortnight;
            }
            else
            {
               IrBiweekly = (AnnualExpectationIR - AccumulatedIR) / nextFortnight;
               IrBiweeklyAdditional = AnnualExpectationAdditionalIr - AnnualExpectationIR;
            }
         }
         else if (totalAnnualSalary > 200000 && totalAnnualSalary <= 350000)
         {
            BaseTax = 15000.00m;
            AnnualIr = ((totalAnnualSalary - 200000) * 0.20m);
            AnnualAdditionalIr = ((totalAnnualAdditionalPayment - 20000) * 0.20m);

            AnnualExpectationIR = AnnualIr + BaseTax;
            AnnualExpectationAdditionalIr = AnnualAdditionalIr + BaseTax;

            if (additionalPayment == 0)
            {
               IrBiweekly = (AnnualExpectationIR - AccumulatedIR) / nextFortnight;
            }
            else
            {
               IrBiweekly = (AnnualExpectationIR - AccumulatedIR) / nextFortnight;
               IrBiweeklyAdditional = AnnualExpectationAdditionalIr - AnnualExpectationIR;
            }
         }
         else if (totalAnnualSalary > 350000 && totalAnnualSalary <= 500000)
         {
            BaseTax = 45000.00m;
            AnnualIr = ((totalAnnualSalary - 350000) * 0.25m);
            AnnualAdditionalIr = ((totalAnnualAdditionalPayment - 350000) * 0.25m);

            AnnualExpectationIR = AnnualIr + BaseTax;
            AnnualExpectationAdditionalIr = AnnualAdditionalIr + BaseTax;

            if (additionalPayment == 0)
            {
               IrBiweekly = (AnnualExpectationIR - AccumulatedIR) / nextFortnight;
            }
            else
            {
               IrBiweekly = (AnnualExpectationIR - AccumulatedIR) / nextFortnight;
               IrBiweeklyAdditional = AnnualExpectationAdditionalIr - AnnualExpectationIR;
            }
         }
         else
         {
            BaseTax = 82500.00m;
            AnnualIr = ((totalAnnualSalary - 500000) * 0.30m);
            AnnualAdditionalIr = ((totalAnnualAdditionalPayment - 500000) * 0.30m);

            AnnualExpectationIR = AnnualIr + BaseTax;
            AnnualExpectationAdditionalIr = AnnualAdditionalIr + BaseTax;

            if (additionalPayment == 0)
            {
               IrBiweekly = (AnnualExpectationIR - AccumulatedIR) / nextFortnight;
            }
            else
            {
               IrBiweekly = (AnnualExpectationIR - AccumulatedIR) / nextFortnight;
               IrBiweeklyAdditional = AnnualExpectationAdditionalIr - AnnualExpectationIR;
            }
         }

         // El saldo a favor se liquida en el cierre anual.
         if (IrBiweekly < 0) IrBiweekly = 0;
         if (IrBiweeklyAdditional < 0) IrBiweeklyAdditional = 0;

         return new IrCalculationResult(
             biweeklyInss + inssAdditionalPayment,
             IrBiweekly + IrBiweeklyAdditional
         );
      }

      public async Task RegisterOrdinaryPayrollForCollaborator(Guid payrollId, Collaborator collaborator, CancellationToken cancellationToken)
      {
         #region Primera Validación de apertura

         var payrollCreated = await _unitOfWork.Payrolls.Entities
             .Where(payroll => payroll.Id == payrollId)
             .FirstOrDefaultAsync(cancellationToken);

         if (payrollCreated is null)
         {
            _logger.LogInformation("No pudistmos encontrar el registro de nomina para hacer el insert de calculos");
            return;
         }

         var salary = await _unitOfWork.Salaries.Entities
             .Include(salary => salary.Collaborator)
                 .ThenInclude(salary => salary.WorkingInformation)
             .Where(salary => salary.EndDate == null)
             .Where(salary => salary.SalaryType == SalaryType.Fixed)
             .Where(salary => salary.CollaboratorId == collaborator.Id)
             .FirstOrDefaultAsync(cancellationToken);

         if (salary is null)
         {
            _logger.LogInformation("No pudistmos encontrar la información salarial del colaborador con cedula => {identificacion}", collaborator.IdentificationNumber);
            return;
         }

         #endregion

         DateOnly entryDate = salary.Collaborator.WorkingInformation.EntryDate;
         DateOnly payrollStart = payrollCreated.StartDate;
         DateOnly payrollEnd = payrollCreated.EndDate;

         int daysWorked = 15;

         if (entryDate > payrollStart) daysWorked = payrollEnd.DayNumber - entryDate.DayNumber + 1;
         else daysWorked = 15;

         if (daysWorked < 0) daysWorked = 0;
         if (daysWorked > 15) daysWorked = 15;

         decimal monthlySalary = salary.AmountInLocal;
         decimal BiweeklySalary = monthlySalary / 2;
         decimal dailySalary = monthlySalary / 30;

         decimal ProportionalBiweeklySalary = dailySalary * daysWorked;

         int YearAntique = 0;
         decimal Bonus = 0.0m;
         decimal Antique = 0.0m;
         decimal Overtime = 0.0m;
         decimal Commissions = 0.0m;
         decimal NumberOfOvertime = 0.0m;

         #region Aplicamos antigüedad si la empresa acumula antigüeda.

         if (collaborator.WorkingInformation.BranchInfo.DoesGenerateSeniority)
         {
            var (antique, yearsOfService) = CalculateAntique(BiweeklySalary, payrollEnd, entryDate);
            Antique = antique;
            YearAntique = yearsOfService;
         }

         #endregion

         decimal GrossSalary = ProportionalBiweeklySalary;
         decimal TotalIncome = ProportionalBiweeklySalary + Overtime + Bonus + Commissions + Antique;

         var TaxInformation = await _unitOfWork.IncomeTaxAccrual.Entities
             .Where(income => income.CollaboratorId == collaborator.Id)
             .OrderByDescending(income => income.CreatedAt)
             .FirstOrDefaultAsync(cancellationToken);

         if (TaxInformation is null)
         {
            _logger.LogInformation("No se encontro la información de acumlado del colaborador con identificaión: {identificaion}", collaborator.IdentificationNumber);
            return;
         }

         var numberFortnights = TaxInformation.FlagNumberOfFortnights;

         var (BiweeklyInss, BiweeklyIr) = await CalculateIr(
             TaxInformation.FlagNumberOfFortnights ?? 24,
             TaxInformation.FlagSalaryEarned ?? 0.0m,
             TaxInformation.FlagAccumulatedIR ?? 0.0m,
             TotalIncome
         );

         var AdditionalDeducctions = new DeductionsAdditionalData()
         {
            Absences = 0.0m,
            CashShortage = 0.0m,
            ChildSupportGarnishment = 0.0m,
            ChristmasBonusAdvance = 0.0m,
            DeductionForLossesBulk = 0.0m,
            JudicialSeizures = 0.0m,
            LateArrivals = 0.0m,
            Loans = 0.0m,
            OtherDeductions = 0.0m,
            Purisima = 0.0m,
            SalaryAdvance = 0.0m,
            Sanction = 0.0m,
            UniformDeduction = 0.0m
         };

         #region Deducciones Activas aqui

         var deductionsActive = await _unitOfWork.Deductions.Entities
             .Where(deduction => deduction.CollaboratorId == collaborator.Id)
             .Where(deduction => deduction.Status == DeductionStatus.Progress)
             .ToListAsync(cancellationToken);

         foreach (var deduction in deductionsActive)
         {
            if (deduction.Type == DeductionType.Loans)
            {
               AdditionalDeducctions.Loans += deduction.FortnightlyAmount ?? 0.0m;
            }

            if (deduction.Type == DeductionType.Purisima)
            {
               AdditionalDeducctions.Purisima += deduction.FortnightlyAmount ?? 0.0m;
            }

            if (deduction.Type == DeductionType.OtherDeductions)
            {
               AdditionalDeducctions.OtherDeductions += deduction.FortnightlyAmount ?? 0.0m;
            }

            if (deduction.Type == DeductionType.JudicialSeizures)
            {
               var exchangeRate = await _unitOfWork.ValidityDeductions.Entities
               .Where(v => v.Status)
               .Where(v => v.EndDate == null)
               .Where(v => v.Type == TaxType.ExchangeRate)
               .FirstOrDefaultAsync(default);

               if (exchangeRate is null)
               {
                  _logger.LogInformation("❌No se pudo consultar la mesa de cambio");
                  return;
               }

               decimal percentage = deduction.Percentage ?? 1;
               decimal baseAmount = TotalIncome - (BiweeklyInss + BiweeklyIr);
               decimal amountToDeduct = Math.Round(baseAmount * (percentage / 100m), 2, MidpointRounding.AwayFromZero);

               if (amountToDeduct > (deduction.TotalBalance ?? 0))
               {
                  amountToDeduct = deduction.TotalBalance ?? 0;
               }
               AdditionalDeducctions.JudicialSeizures += amountToDeduct;

               await _unitOfWork.DeductionPaymentHistories.RegisterDeductionPaymentHistory(new()
               {
                  DeductionId = deduction.Id,
                  AmountPaid = amountToDeduct,
                  AmountPaidInDollars = amountToDeduct / exchangeRate.Value,
                  Status = DeductionPaymentStatus.Pending,
                  Origin = SourceDeductionPayment.Payroll,
                  Currency = deduction.Currency,
                  PayrollId = payrollCreated.Id,
                  PaymentDate = DateTime.Now,
               });
               continue;
            }

            await _unitOfWork.DeductionPaymentHistories.RegisterDeductionPaymentHistory(new()
            {
               DeductionId = deduction.Id,
               AmountPaid = deduction.FortnightlyAmount ?? 0.0m,
               AmountPaidInDollars = (deduction.FortnightlyAmount ?? 0.0m) / 36.6243m,

               Status = DeductionPaymentStatus.Pending,
               Origin = SourceDeductionPayment.Payroll,

               Currency = deduction.Currency,

               PayrollId = payrollCreated.Id,
               PaymentDate = DateTime.Now,
            });
         }
         #endregion

         decimal totalDeductionsAdditionals =
             AdditionalDeducctions.Loans
             + AdditionalDeducctions.Purisima
             + AdditionalDeducctions.ChildSupportGarnishment
             + AdditionalDeducctions.SalaryAdvance
             + AdditionalDeducctions.ChristmasBonusAdvance
             + AdditionalDeducctions.JudicialSeizures
             + AdditionalDeducctions.UniformDeduction
             + AdditionalDeducctions.CashShortage
             + AdditionalDeducctions.OtherDeductions
             + AdditionalDeducctions.DeductionForLossesBulk
             + AdditionalDeducctions.Absences
             + AdditionalDeducctions.Sanction
             + AdditionalDeducctions.LateArrivals;

         #region Asignación de viaticos

         var asssineds = await _unitOfWork.AssignedTravelExpenses.Entities
             .Include(asssined => asssined.Collaborator)
             .Where(assigned => assigned.CollaboratorId == collaborator.Id && assigned.EndDate == null)
             .Include(asssined => asssined.TypeIncome)
             .ToListAsync(cancellationToken);

         decimal Lodging = 0.0m;
         decimal Transport = 0.0m;
         decimal FoodTravelAllowance = 0.0m;
         decimal totalAssigned = 0.0m;

         int DEFAULT_TOTAL_WORK_DAYS = 0;

         var holidays = await _unitOfWork.Holidays.Entities
             .Where(day => day.IsActive)
             .ToListAsync(default);

         //Recorremos los dias
         for (DateOnly date = payrollStart; date <= payrollEnd; date = date.AddDays(1))
         {
            bool isHoliday = holidays.Any(holiday =>
                holiday.Day == date.Day &&
                holiday.Month == date.Month &&
                (
                    holiday.IsGlobal ||
                    holiday.BranchId == collaborator.WorkingInformation.CompanyBranchId
                )
            );

            if (isHoliday)
            {
               continue;
            }

            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
               continue;
            }

            if (!collaborator.DoesWorkSaturdays && date.DayOfWeek == DayOfWeek.Saturday)
            {
               continue;
            }

            DEFAULT_TOTAL_WORK_DAYS++;
         }


         foreach (var current in asssineds)
         {
            switch (current.TypeIncome.IncomeCode)
            {
               case "ALW_TRANSPORT":
                  {
                     Transport = current.AmountInLocalCurrency;
                     totalAssigned += Transport;
                     break;
                  }
               case "ALW_HOUSING":
                  {
                     Lodging = current.AmountInLocalCurrency * DEFAULT_TOTAL_WORK_DAYS;
                     totalAssigned += Lodging;
                     break;
                  }
               case "ALW_MEAL":
                  {
                     FoodTravelAllowance = current.AmountInLocalCurrency;
                     totalAssigned += FoodTravelAllowance;
                     break;
                  }
               default:
                  {
                     continue;
                  }
            }
         }

         #endregion

         decimal TotalLegalDeductions = BiweeklyInss + BiweeklyIr;
         decimal TotalDeducctions = TotalLegalDeductions + totalDeductionsAdditionals;

         Lodging *= DEFAULT_TOTAL_WORK_DAYS;
         Transport *= DEFAULT_TOTAL_WORK_DAYS;
         FoodTravelAllowance *= DEFAULT_TOTAL_WORK_DAYS;
         totalAssigned = Lodging + Transport + FoodTravelAllowance;

         decimal TotalToPay = TotalIncome - TotalDeducctions + totalAssigned;

         //Calcular Aguinaldo y vacaciones 🚩
         var payload = new OrdinaryPayroll()
         {
            Id = Guid.NewGuid(),
            CollaboratorId = collaborator.Id,
            PayrollId = payrollId,
            BiweeklySalary = BiweeklySalary,

            Overtime = Overtime,
            NumberOvertime = NumberOfOvertime,
            Bonus = Bonus,
            Commissions = Commissions,
            Antique = Antique,

            GrossSalary = GrossSalary,
            TotalIncome = TotalIncome,

            Inss = BiweeklyInss,
            Ir = BiweeklyIr,

            TotalLegalDeductions = TotalLegalDeductions,
            DeductionsAdditionalData = JsonSerializer.Serialize(AdditionalDeducctions),
            TotalDeducctions = TotalDeducctions,

            TotalTravelExpenses = totalAssigned,
            Feeding = FoodTravelAllowance,
            Lodging = Lodging,
            Transport = Transport,
            TotalToPay = TotalToPay,
         };

         var PayrollRegistered = await _unitOfWork.OrdinaryPayrolls.RegisterCollaboratorInTheOrdinaryPayroll(payload);

         #region Registrar informe de viaticos.

         await _unitOfWork.RecordsTravelExpensePayments.RegisterRecordsTravelExpensePayment(new()
         {
            CollaboratorId = collaborator.Id,
            PayrollId = payrollId,
            PaidDays = DEFAULT_TOTAL_WORK_DAYS,
            Feeding = FoodTravelAllowance,
            Transport = Transport,
            Lodging = Lodging
         });

         #endregion

         #region Iniciar proceso de acumulados

         //Registrar el acumulado para la siguiente apertura de quincena

         await _unitOfWork.IncomeTaxAccrual.RegisterIncomeTaxAccrual(new()
         {
            AccumulatedIR = TaxInformation.FlagAccumulatedIR ?? 0.0m,
            SalaryEarned = TaxInformation.FlagSalaryEarned ?? 0.0m,
            NumberOfFortnights = TaxInformation.FlagNumberOfFortnights ?? 24,

            FlagAccumulatedIR = (TaxInformation.FlagNumberOfFortnights ?? 24) == 1 ? 0.0m : (TaxInformation.FlagAccumulatedIR ?? 0.0m) + BiweeklyIr,
            FlagSalaryEarned = (TaxInformation.FlagNumberOfFortnights ?? 24) == 1 ? 0.0m : (TaxInformation.FlagSalaryEarned ?? 0.0m) + (TotalIncome - BiweeklyInss),
            FlagNumberOfFortnights = (TaxInformation.FlagNumberOfFortnights ?? 24) == 1 ? 24 : (TaxInformation.FlagNumberOfFortnights - 1),

            CollaboratorId = collaborator.Id,
            PayrollId = payrollCreated.Id,
            AccumulatedSeniority = 0.0m,
         });

         #endregion


         #region Calcular Inatec e inss patronal


         #endregion Calcular Inatec e inss patronal

      }
   }
}