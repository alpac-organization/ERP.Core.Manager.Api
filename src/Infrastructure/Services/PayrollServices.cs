using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;


namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    public class PayrollServices(IUnitOfWork _unitOfWork, ICalculatorDeductions _calculatorDeductions, ILogger<CalculatorDeductions> _logger) : IPayrollServices
    {
        public async Task<int> CalculateNumberDaysToAssignedTravelExpenses(Collaborator collaborator, DateOnly payrollStart, DateOnly payrollEnd)
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

            DateOnly entryDate      = salary.Collaborator.WorkingInformation.EntryDate;
            DateOnly payrollStart   = payrollCreated.StartDate;
            DateOnly payrollEnd     = payrollCreated.EndDate;

           int daysWorked = 15;

            if (entryDate > payrollStart) daysWorked = payrollEnd.DayNumber - entryDate.DayNumber + 1;
            else daysWorked = 15;

            if (daysWorked < 0)     daysWorked = 0;
            if (daysWorked > 15)    daysWorked = 15;

            decimal monthlySalary   = salary.AmountInLocal;
            decimal BiweeklySalary  = monthlySalary / 2;
            decimal dailySalary     = monthlySalary / 30;

            decimal ProportionalBiweeklySalary = dailySalary * daysWorked;

            decimal Bonus = 0.0m;
            decimal Antique = 0.0m;
            decimal Overtime = 0.0m;
            decimal Commissions = 0.0m;
            decimal NumberOfOvertime = 0.0m;

            #region Aplicamos antigüedad si la empresa acumula antigüeda.
            
            if (collaborator.WorkingInformation.BranchInfo.DoesGenerateSeniority)
            {
                Antique = _calculatorDeductions.CalculateAntique(monthlySalary, payrollStart, entryDate);
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

            var (BiweeklyInss, BiweeklyIr) = await _calculatorDeductions.CalculateIr(
                TaxInformation.FlagNumberOfFortnights ?? 24,
                TaxInformation.FlagSalaryEarned       ?? 0.0m,
                TaxInformation.FlagAccumulatedIR      ?? 0.0m,
                TotalIncome,
                cancellationToken
            );

            var AdditionalDeducctions = new DeductionsAdditionalData()
            {
                Absences = 0.0m,
                CashShortage  = 0.0m,
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

            foreach(var deduction in deductionsActive)
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
                    DeductionId         = deduction.Id,

                    AmountPaid          = deduction.FortnightlyAmount ?? 0.0m,
                    AmountPaidInDollars = (deduction.FortnightlyAmount ?? 0.0m) / 36.6243m,
                    
                    Status              = DeductionPaymentStatus.Pending,
                    Origin              = SourceDeductionPayment.Payroll,
                    Currency            = deduction.Currency,
                    PayrollId           = payrollCreated.Id,
                    PaymentDate         = DateTime.Now,
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
            int totalWorkDays = await CalculateNumberDaysToAssignedTravelExpenses(collaborator, payrollStart, payrollEnd);

            var asssineds = await _unitOfWork.AssignedTravelExpenses.Entities
                .Include(asssined => asssined.Collaborator)
                .Where(assigned => assigned.CollaboratorId == collaborator.Id && assigned.EndDate == null)
                .Include(asssined => asssined.TypeIncome)
                .ToListAsync(cancellationToken);

            decimal Lodging             = 0.0m;
            decimal Transport           = 0.0m;
            decimal FoodTravelAllowance = 0.0m;
            decimal totalAssigned       = 0.0m;

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
                    case "ALW_HOUSING" :
                    {
                        Lodging = current.AmountInLocalCurrency;
                        totalAssigned += Lodging;
                        break;
                    }
                    case "ALW_MEAL":
                    {
                        FoodTravelAllowance  = current.AmountInLocalCurrency;
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

            Lodging             *= totalWorkDays;
            Transport           *= totalWorkDays;
            FoodTravelAllowance *= totalWorkDays;
            totalAssigned       = Lodging + Transport + FoodTravelAllowance;

            decimal TotalToPay = TotalIncome - TotalDeducctions + totalAssigned;

            //Calcular Aguinaldo y vacaciones 🚩
            var payload = new OrdinaryPayroll()
            {
                Id = Guid.NewGuid(),
                CollaboratorId           = collaborator.Id,
                PayrollId                = payrollId,
                BiweeklySalary           = BiweeklySalary,                

                Overtime                 = Overtime,
                NumberOvertime           = NumberOfOvertime,
                Bonus                    = Bonus,
                Commissions              = Commissions,
                Antique                  = Antique,

                GrossSalary              = GrossSalary,
                TotalIncome              = TotalIncome,
                
                Inss                     = BiweeklyInss,
                Ir                       = BiweeklyIr,
    
                TotalLegalDeductions     = TotalLegalDeductions,
                DeductionsAdditionalData = JsonSerializer.Serialize(AdditionalDeducctions),
                TotalDeducctions         = TotalDeducctions,

                TotalTravelExpenses      = totalAssigned,
                Feeding                  = FoodTravelAllowance,
                Lodging                  = Lodging,
                Transport                = Transport,
                TotalToPay               = TotalToPay,
            };

            var PayrollRegistered = await _unitOfWork.OrdinaryPayrolls.RegisterCollaboratorInTheOrdinaryPayroll(payload);
            
            #region Registro del inss

            #endregion

            #region Registro de vacaciones

            var vacationControl = await _unitOfWork.Vacations.Entities    
                .Where(vac => vac.CollaboratorId == collaborator.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (vacationControl is null)
            {
                _logger.LogInformation("Este colaborador no cuenta con control de vacaciones: {identification}", collaborator.IdentificationNumber);
                return;
            }

            decimal vacationAmountInCordobas = vacationControl.AvailableVacations * dailySalary;
            decimal vacationAmountInDollars =  (vacationControl.AvailableVacations * dailySalary) / 36.6243m;

            await _unitOfWork.VacationAccruals.RegisterVacationAccrual(new()
            {
                BeginningBalance = vacationControl.AvailableVacations,
                FinalBalance = vacationControl.AvailableVacations,
                PayrollId = payrollCreated.Id,
                CollaboratorId = collaborator.Id,
                AvailableVacations  = vacationControl.AvailableVacations,
                EquivalentQuantity = vacationAmountInCordobas,
                EquivalentQuantityInDollars = vacationAmountInDollars
            });

            #endregion

            #region Registrar informe de viaticos.

            //Registro de pagos de viaticos.
            await _unitOfWork.RecordsTravelExpensePayments.RegisterRecordsTravelExpensePayment(new()
            {
                CollaboratorId = collaborator.Id,
                PayrollId      = payrollId,
                PaidDays       = totalWorkDays,
                Feeding        = FoodTravelAllowance,
                Transport      = Transport,
                Lodging        = Lodging
            });

            #endregion

            #region Iniciar proceso de acumulados

            //Registrar el acumulado para la siguiente apertura de quincena
            await _unitOfWork.IncomeTaxAccrual.RegisterIncomeTaxAccrual(new()
            {
                AccumulatedIR           = TaxInformation.FlagAccumulatedIR      ?? 0.0m,
                SalaryEarned            = TaxInformation.FlagSalaryEarned       ?? 0.0m,
                NumberOfFortnights      = TaxInformation.FlagNumberOfFortnights ?? 24,

                FlagAccumulatedIR       = (TaxInformation.FlagNumberOfFortnights ?? 24) == 1 ? 0.0m  : (TaxInformation.FlagAccumulatedIR ?? 0.0m)  + BiweeklyIr,
                FlagSalaryEarned        = (TaxInformation.FlagNumberOfFortnights ?? 24) == 1 ? 0.0m  : (TaxInformation.FlagSalaryEarned  ?? 0.0m)  + (TotalIncome - BiweeklyInss),
                FlagNumberOfFortnights  = (TaxInformation.FlagNumberOfFortnights ?? 24) == 1 ? 24    : (TaxInformation.FlagNumberOfFortnights - 1),

                PayrollId               = payrollCreated.Id,
                CollaboratorId          = collaborator.Id,

                AccumulatedSeniority    = 0.0m,
                //Agregar bandera de antiguedad.
            });
            
            #endregion
        }
    }
}