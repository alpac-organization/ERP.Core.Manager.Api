using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    #pragma warning disable CA1873

    public class CalculatorDeductions(IUnitOfWork _unitOfWork, ILogger<CalculatorDeductions> _logger) : ICalculatorDeductions
    {
        public decimal CalculateAntique(decimal monthlySalary, DateTime payrollStartDate, DateTime collaboratorEntryDate)
        {
            _logger.LogInformation("Iniciando Proceso para calcular antiguedad✅");

            DateTime EntryDate = collaboratorEntryDate;

            int yearsOfService = payrollStartDate.Year - EntryDate.Year;    
            if (EntryDate.Date > payrollStartDate.AddYears(-yearsOfService)) yearsOfService--;

            decimal seniorityPercentage = yearsOfService switch
            {
                <= 0 => 0.00m,
                1    => 0.03m,
                2    => 0.05m,
                3    => 0.07m,
                4    => 0.09m,
                5    => 0.10m,
                6    => 0.11m,
                7    => 0.12m,
                8    => 0.13m,
                9    => 0.14m,
                10   => 0.15m,
                11   => 0.155m,
                12   => 0.16m,
                13   => 0.165m,
                14   => 0.17m,
                15   => 0.175m,
                16   => 0.18m,
                17   => 0.185m,
                18   => 0.19m,
                19   => 0.195m,
                _    => 0.20m
            };

            return seniorityPercentage * monthlySalary;
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
        public async Task<IrCalculationResult> CalculateIr(int NFortnight , decimal AccumulatedAccrued, decimal AccumulatedIR, decimal GrossSalary, CancellationToken cancellationToken, bool isSudsidy = false)
        {
            var nextFortnight = NFortnight;

            decimal biweeklyInss = 0.0m;

            if (!isSudsidy)
            {
                biweeklyInss = await CalculateInss(GrossSalary, cancellationToken);
            }

            //Salario quincenal libre de inss.
            decimal netSalary = GrossSalary - biweeklyInss;
            decimal AnnualSalary = netSalary * nextFortnight;

            decimal totalAnnualSalary = AnnualSalary + AccumulatedAccrued;
            decimal AnnualIr;

            //Agregar regla del ir
            decimal BaseTax;
            decimal AnnualExpectationIR = 0.0m;
            decimal IrBiweekly = 0.0m;


            if (totalAnnualSalary <= 100000)
                AnnualIr = 0;
            else if (totalAnnualSalary > 100000 && totalAnnualSalary <= 200000)
            {
                BaseTax = 0;
                AnnualIr = ((totalAnnualSalary - 100000) * 0.15m);
                AnnualExpectationIR = AnnualIr + BaseTax;

                IrBiweekly = (AnnualExpectationIR - AccumulatedIR) / nextFortnight;
            }
            else if (totalAnnualSalary > 200000 && totalAnnualSalary <= 350000)
            {
                BaseTax = 15000.00m;
                AnnualIr = ((totalAnnualSalary - 200000) * 0.20m);
                AnnualExpectationIR = AnnualIr + BaseTax;

                IrBiweekly = (AnnualExpectationIR - AccumulatedIR) / nextFortnight;
            }
            else if (totalAnnualSalary > 350000 && totalAnnualSalary <= 500000)
            {
                BaseTax = 45000.00m;
                AnnualIr = ((totalAnnualSalary - 350000) * 0.25m);
                AnnualExpectationIR = AnnualIr + BaseTax;

                IrBiweekly = (AnnualExpectationIR - AccumulatedIR) / nextFortnight;
            }
            else
            {
                BaseTax = 82500.00m;
                AnnualIr = ((totalAnnualSalary - 500000) * 0.30m);

                AnnualExpectationIR = AnnualIr + BaseTax;
                IrBiweekly = (AnnualExpectationIR - AccumulatedIR) / nextFortnight;
            }

            return new IrCalculationResult(
                biweeklyInss,
                IrBiweekly
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

            DateTime entryDate      = salary.Collaborator.WorkingInformation.EntryDate;
            DateTime payrollStart   = payrollCreated.StartDate;
            DateTime payrollEnd     = payrollCreated.EndDate;

            int daysWorked = 15;

            if (entryDate > payrollStart) daysWorked = (payrollEnd - entryDate).Days + 1;
            else  daysWorked = 15; 

            if (daysWorked < 0)  daysWorked = 0;
            if (daysWorked > 15) daysWorked = 15;

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
                Antique = CalculateAntique(monthlySalary, payrollStart, entryDate);
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
                    PaymentDate         = payrollCreated.EndDate,
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

            decimal Lodging             = 0.0m;
            decimal Transport           = 0.0m;
            decimal FoodTravelAllowance = 0.0m;
            decimal totalAssigned       = 0.0m;

            var DEFAULT_TOTAL_WORK_DAYS = collaborator.DoesWorkSaturdays ? 13 : 11;

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
                        Lodging = current.AmountInLocalCurrency * DEFAULT_TOTAL_WORK_DAYS;
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

            Lodging             *= DEFAULT_TOTAL_WORK_DAYS;
            Transport           *= DEFAULT_TOTAL_WORK_DAYS;
            FoodTravelAllowance *= DEFAULT_TOTAL_WORK_DAYS;
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

            #region Iniciar proceso de acumulados

            //Registrar el acumulado para la siguiente apertura de quincena

            await _unitOfWork.IncomeTaxAccrual.RegisterIncomeTaxAccrual(new()
            {
                AccumulatedIR       = TaxInformation.FlagAccumulatedIR      ?? 0.0m,
                SalaryEarned        = TaxInformation.FlagSalaryEarned       ?? 0.0m,
                NumberOfFortnights  = TaxInformation.FlagNumberOfFortnights ?? 24,

                FlagAccumulatedIR      = (TaxInformation.FlagNumberOfFortnights ?? 24) == 1 ? 0.0m : (TaxInformation.FlagAccumulatedIR ?? 0.0m) + BiweeklyIr,
                FlagSalaryEarned       = (TaxInformation.FlagNumberOfFortnights ?? 24) == 1 ? 0.0m : (TaxInformation.FlagSalaryEarned ?? 0.0m)  + (TotalIncome - BiweeklyInss),
                FlagNumberOfFortnights = (TaxInformation.FlagNumberOfFortnights ?? 24) == 1 ? 24 : (TaxInformation.FlagNumberOfFortnights - 1),

                CollaboratorId      = collaborator.Id,
                PayrollId           = payrollCreated.Id,
                AccumulatedSeniority = 0.0m,
            });
            
            #endregion
        }
    }

    #pragma warning restore CA1873
}