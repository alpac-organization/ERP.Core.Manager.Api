using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
   public class PayrollServices(IUnitOfWork _unitOfWork, ICalculatorDeductions _calculatorDeductions, ILogger<CalculatorDeductions> _logger) : IPayrollServices
   {
      public async Task<List<Collaborator>> ObtainsCollaboratorByType(SalaryType salaryType, Guid companyId, Guid branchId)
      {
         var collabotators = await _unitOfWork.Collaborators.Entities
             .Where(col => col.CompanyId == companyId)
             .Where(col => col.Status != CollaboratorStatus.Inactive)
             .Include(col => col.WorkingInformation)
             .Where(col => col.WorkingInformation.CompanyBranchId == branchId)
             .Include(c => c.Salaries
                 .Where(s => s.EndDate == null && s.SalaryType == salaryType)
             )
             .Where(c => c.Salaries
                 .Any(s => s.EndDate == null && s.SalaryType == salaryType)
             )
             .ToListAsync(default);

         return collabotators;
      }

      public async Task<int> AssignTravelDays(Collaborator collaborator, DateOnly payrollStart, DateOnly payrollEnd)
      {
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

         return DEFAULT_TOTAL_WORK_DAYS;
      }

      public async Task AssignTravelAllowance(Collaborator collaborator, List<TravelExpenses> travelExpenses)
      {
         //Mandar a llamar la tasa de cambio registrada en nuestra base de datos.

         foreach (var travel in travelExpenses)
         {
            if (travel.IncomeAmount == 0)
            {
               _logger.LogInformation("La cantidad asignada no puede ser 0");
               continue;
            }
            if (string.IsNullOrEmpty(travel.TypeIncomeId.ToString()))
            {

               _logger.LogInformation("El tipo de ingreso es obligatorio");
               continue;
            }

            var history = new AssignedTravelExpenses
            {
               Id = Guid.NewGuid(),
               AmountInDollars = travel.IncomeAmount / 36.6243m,
               AmountInLocalCurrency = travel.IncomeAmount,
               CollaboratorId = collaborator.Id,
               Currency = Currency.NIO,
               TypeIncomeId = travel.TypeIncomeId,
               StartDate = DateTime.Now,
               EndDate = null
            };

            //Rollback
            await _unitOfWork.AssignedTravelExpenses.RegisterAssignedTravelExpenses(history);
         }
      }

      public async Task AssignVacationControl(Collaborator collaborator)
      {
         var daysElapsed = CalculatorUtils.CalculateDaysElapsedCommercial(collaborator.WorkingInformation.EntryDate);

         decimal generated = Math.Round((decimal)(daysElapsed * 30.0 / 360.0), 4);

         Vacation vacation = new()
         {
            CollaboratorId = collaborator.Id,
            EnjoyedVacation = 0,
            GeneredVacation = generated,
            AvailableVacations = generated,
         };

         await _unitOfWork.Vacations.RegisterVacationControl(vacation);

         //Rollback
      }

      public async Task<bool> AssignSalary(Collaborator collaborator, SalaryInformation salaryInformation)
      {
         var salary = new Salary();

         decimal amountInLocal = 0;
         decimal amountInForeign = 0;

         //Consultando mesa de cambio actual
         var exchangeRate = await _unitOfWork.ValidityDeductions.Entities
             .Where(val => val.Status)
             .Where(val => val.EndDate == null)
             .Where(val => val.Type == TaxType.ExchangeRate)
             .FirstOrDefaultAsync(default);

         if (exchangeRate is null)
         {
            _logger.LogInformation("❌No se pudo consultar la mesa de cambio");
            return false;
         }

         if (salaryInformation.SalaryType != SalaryType.ProfessionalServices)
         {
            decimal amountSalary = salaryInformation?.Salary ?? 0;

            if (salaryInformation!.Currency == Currency.USD)
            {
               amountInLocal = amountSalary * exchangeRate.Value;
               amountInForeign = amountSalary;
            }
            else
            {
               amountInLocal = amountSalary;
               amountInForeign = amountSalary / exchangeRate.Value;
            }

            salary = new Salary()
            {
               CollaboratorId = collaborator.Id,
               Currency = salaryInformation.Currency,
               SalaryType = salaryInformation.SalaryType,
               BankSubCatalogId = salaryInformation.SubCatalogBankId,
               AmountSalary = amountSalary,
               AmountInLocal = amountInLocal,
               AmountInForeign = amountInForeign,
               StartDate = DateTime.Now
            };

            //✅Registro de salario exitoso
            await _unitOfWork.Salaries.RegisterSalary(salary);

            _logger.LogInformation("✅Se registro exitosamente el salario. para colaborador con cedula: {identification}", collaborator.IdentificationNumber);
         }

         //Devolvemos exitoso
         return true;
      }

      public async Task RegisterCollaboratorToPayroll(Guid payrollId, Collaborator collaborator)
      {
         #region Primera Validación de apertura

         var payrollCreated = await _unitOfWork.Payrolls.Entities
             .Where(payroll => payroll.Id == payrollId)
             .FirstOrDefaultAsync(default);

         if (payrollCreated is null)
         {
            _logger.LogInformation("No pudimos encontrar el registro de nomina para hacer el insert de calculos");
            return;
         }

         var salary = await _unitOfWork.Salaries.Entities
             .Include(salary => salary.Collaborator)
                 .ThenInclude(salary => salary.WorkingInformation)
             .Where(salary => salary.EndDate == null)
             .Where(salary => salary.SalaryType == SalaryType.Fixed)
             .Where(salary => salary.CollaboratorId == collaborator.Id)
             .FirstOrDefaultAsync(default);

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
            var (antique, yearsOfService) = _calculatorDeductions.CalculateAntique(BiweeklySalary, payrollEnd, entryDate); ;
            Antique = antique;
            YearAntique = yearsOfService;
         }

         #endregion

         decimal GrossSalary = ProportionalBiweeklySalary;
         decimal TotalIncome = ProportionalBiweeklySalary + Overtime + Commissions + Antique;

         var TaxInformation = await _unitOfWork.IncomeTaxAccrual.Entities
             .Where(income => income.CollaboratorId == collaborator.Id)
             .OrderByDescending(income => income.CreatedAt)
             .FirstOrDefaultAsync(default);


         var numberFortnights = TaxInformation?.FlagNumberOfFortnights ?? 24;

         var (BiweeklyInss, BiweeklyIr) = await _calculatorDeductions.CalculateIr(
             TaxInformation?.FlagNumberOfFortnights ?? 24,
             TaxInformation?.FlagSalaryEarned ?? 0.0m,
             TaxInformation?.FlagAccumulatedIR ?? 0.0m,
             TotalIncome,
            default
         );

         TotalIncome += Bonus;

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
             .ToListAsync(default);

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

         //Saber cuantos dias tiene con derecho a viaticos en base al calendario del mes del colaborador.
         int totalWorkDays = await AssignTravelDays(collaborator, payrollStart, payrollEnd);

         var asssineds = await _unitOfWork.AssignedTravelExpenses.Entities
             .Include(asssined => asssined.Collaborator)
             .Where(assigned => assigned.CollaboratorId == collaborator.Id && assigned.EndDate == null)
             .Include(asssined => asssined.TypeIncome)
             .ToListAsync(default);

         decimal Lodging = 0.0m;
         decimal Transport = 0.0m;
         decimal FoodTravelAllowance = 0.0m;
         decimal totalAssigned = 0.0m;

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
                     Lodging = current.AmountInLocalCurrency;
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

         Lodging *= totalWorkDays;
         Transport *= totalWorkDays;
         FoodTravelAllowance *= totalWorkDays;
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
            YearAntique = YearAntique,

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

         #region Registro del inss


         #endregion

         #region Registro de Aguinaldo

         #endregion

         #region Registro de vacaciones

         var vacationControl = await _unitOfWork.Vacations.Entities
             .Where(vac => vac.CollaboratorId == collaborator.Id)
             .FirstOrDefaultAsync(default);

         if (vacationControl is null)
         {
            _logger.LogInformation("Este colaborador no cuenta con control de vacaciones: {identification}", collaborator.IdentificationNumber);
            return;
         }

         decimal vacationAmountInCordobas = vacationControl.AvailableVacations * dailySalary;
         decimal vacationAmountInDollars = (vacationControl.AvailableVacations * dailySalary) / 36.6243m;

         // await _unitOfWork.VacationAccruals.RegisterVacationAccrual(new()
         // {
         //     BeginningBalance = vacationControl.AvailableVacations,
         //     FinalBalance = vacationControl.AvailableVacations,
         //     PayrollId = payrollCreated.Id,
         //     CollaboratorId = collaborator.Id,
         //     AvailableVacations  = vacationControl.AvailableVacations,
         //     EquivalentQuantity = vacationAmountInCordobas,
         //     EquivalentQuantityInDollars = vacationAmountInDollars
         // });

         #endregion

         #region Registrar informe de viaticos.

         //Registro de pagos de viaticos.
         await _unitOfWork.RecordsTravelExpensePayments.RegisterRecordsTravelExpensePayment(new()
         {
            CollaboratorId = collaborator.Id,
            PayrollId = payrollId,
            PaidDays = totalWorkDays,
            Feeding = FoodTravelAllowance,
            Transport = Transport,
            Lodging = Lodging
         });

         #endregion

         #region Iniciar proceso de acumulados

         //Registrar el acumulado para la siguiente apertura de quincena
         if (collaborator.IsFirstTimeRegister)
         {
            await _unitOfWork.IncomeTaxAccrual.RegisterIncomeTaxAccrual(new()
            {
               AccumulatedIR = 0.0m,
               SalaryEarned = 0.0m,
               NumberOfFortnights = 24,

               FlagAccumulatedIR = BiweeklyIr,
               FlagSalaryEarned = TotalIncome - BiweeklyInss,
               FlagNumberOfFortnights = 23,

               PayrollId = payrollCreated.Id,
               CollaboratorId = collaborator.Id,

               AccumulatedSeniority = 0.0m,
            });

            collaborator.IsFirstTimeRegister = false;
            await _unitOfWork.Collaborators.UpdateAsync(collaborator);
         }
         else
         {
            await _unitOfWork.IncomeTaxAccrual.RegisterIncomeTaxAccrual(new()
            {
               AccumulatedIR = TaxInformation?.FlagAccumulatedIR ?? 0.0m,
               SalaryEarned = TaxInformation?.FlagSalaryEarned ?? 0.0m,
               NumberOfFortnights = TaxInformation?.FlagNumberOfFortnights ?? 24,

               FlagAccumulatedIR = (TaxInformation?.FlagNumberOfFortnights ?? 24) == 1 ? 0.0m : (TaxInformation?.FlagAccumulatedIR ?? 0.0m) + BiweeklyIr,
               FlagSalaryEarned = (TaxInformation?.FlagNumberOfFortnights ?? 24) == 1 ? 0.0m : (TaxInformation?.FlagSalaryEarned ?? 0.0m) + (TotalIncome - BiweeklyInss),
               FlagNumberOfFortnights = (TaxInformation?.FlagNumberOfFortnights ?? 24) == 1 ? 24 : ((TaxInformation?.FlagNumberOfFortnights ?? 24) - 1),

               PayrollId = payrollCreated.Id,
               CollaboratorId = collaborator.Id,

               AccumulatedSeniority = 0.0m,
            });
         }
         #endregion
      }

      public async Task RegisterCollaboratorToVigemsaProfessional(Guid payrollId, Collaborator collaborator)
      {
         var payrollCreated = await _unitOfWork.Payrolls.Entities
             .Where(pay => pay.Id == payrollId)
             .FirstOrDefaultAsync(default);

         if (payrollCreated is null)
         {
            _logger.LogInformation("");
            return;
         }

         var salary = await _unitOfWork.Salaries.Entities
            .Include(salary => salary.Collaborator)
                .ThenInclude(salary => salary.WorkingInformation)
            .Where(salary => salary.EndDate == null)
            .Where(salary => salary.SalaryType == SalaryType.Fixed)
            .Where(salary => salary.CollaboratorId == collaborator.Id)
            .FirstOrDefaultAsync(default);

         if (salary is null)
         {
            _logger.LogInformation("No pudimos encontrar la información salarial del colaborador con cedula => {identificacion}", collaborator.IdentificationNumber);
            return;
         }


         var additionalData = new VigemsaAdditionalData()
         {
            TotalHoursWorked = 0.0m,
            TotalNumberShiftsPerformed = 0.0m,
         };


         var payload = new ProfessionalServicesPayroll()
         {
            Id = Guid.NewGuid(),
            Ir = 0.0m,
            Inss = 0.0m,
            GrossSalary = 0.0m,

            Vacations = 0.0m,
            TotalToPay = 0.0m,
            ChristmasBonus = 0.0m,
            TotalLegalDeductions = 0.0m,
            VigemsaAdditionalData = JsonSerializer.Serialize(additionalData),

            PayrollId = payrollCreated.Id,
            CollaboratorId = collaborator.Id
         };

         await _unitOfWork.ProfessionalServicesPayrolls.RegisterCollaboratorInTheProfessionalServicesPayroll(payload);

         DateOnly payrollStart = payrollCreated.StartDate;
         DateOnly payrollEnd = payrollCreated.EndDate;

         //Crear registros de fechas para el control de asistencias
         // for (DateOnly Current in )
         // {

         // }





         //Se finaliza el proceso de registro a la nomina prestacionada.
      }

      public async Task RegisterCollaboratorToAvasaTransport()
      {

      }
   }
}