using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    public class CalculatorDeductions(IUnitOfWork _unitOfWork, ILogger<CalculatorDeductions> _logger) : ICalculatorDeductions
    {

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
        public async Task<IrCalculationResult> CalculateIrToNextProcess(int NFortnight , decimal AccumulatedAccrued, decimal AccumulatedIR, decimal GrossSalary, CancellationToken cancellationToken)
        {
            var nextFortnight = NFortnight;

            var biweeklyInss = await CalculateInss(GrossSalary, cancellationToken);

            //Salario quincenal libre de inss.
            decimal netSalary = GrossSalary - biweeklyInss;
            decimal AnnualSalary = netSalary * nextFortnight;

            decimal totalAnnualSalary = AnnualSalary + AccumulatedAccrued;
            decimal AnnualIr;

            //Aqui vamos bien

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

        public async Task<decimal> CalculateIr(decimal GrossSalary, int daysWorked, CancellationToken cancellationToken)
        {
            //Para sacar el ir proporcional y debemos aprender a tomar en cuenta las horas extras y bonos
            if (daysWorked <= 0 || GrossSalary <= 0) return 0;

            //Iniciando proceso de calculo de ir
            var InssBiweekly = await CalculateInss(GrossSalary, cancellationToken);
            var BiweeklyTaxableBase = GrossSalary - InssBiweekly;

            decimal StandardizedBiweeklyBase = (BiweeklyTaxableBase / daysWorked) * 15;

            //Sacar la anualidad salarial para aplicar regla del ir.
            decimal AnnualSalary = StandardizedBiweeklyBase * 24;
            decimal AnnualIr;

            // Tabla del ir.
            if (AnnualSalary <= 100000)
                AnnualIr = 0;
            else if (AnnualSalary <= 200000)
                AnnualIr = (AnnualSalary - 100000) * 0.15m;
            else if (AnnualSalary <= 350000)
                AnnualIr = ((AnnualSalary - 200000) * 0.20m) + 15000;
            else if (AnnualSalary <= 500000)
                AnnualIr = ((AnnualSalary - 350000) * 0.25m) + 45000;
            else
                AnnualIr = ((AnnualSalary - 500000) * 0.30m) + 82500;

            decimal StandardBiweeklyIr = AnnualIr / 24;
            decimal irProporcionalFinal = StandardBiweeklyIr / 15 * daysWorked;

            return irProporcionalFinal;
        }

        public async Task RegisterOrdinaryPayrollForCollaborator(Guid payrollId, Collaborator collaborator, CancellationToken cancellationToken)
        {
            //Id de la nomina a la que vamos hacer el registro de insert.
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
                #pragma warning disable CA1873

                _logger.LogInformation("No pudistmos encontrar la información salarial del colaborador con cedula => {identificacion}", collaborator.IdentificationNumber);
                
                #pragma warning restore CA1873
                
                return;
            }

            //Fecha de ingreso a la empresa a laborar
            DateTime entryDate = salary.Collaborator.WorkingInformation.EntryDate;
            DateTime payrollStart = payrollCreated.StartDate;

            DateTime payrollEnd = payrollCreated.EndDate ?? payrollStart.AddDays(14);

            int daysWorked = 15;

            //Calculamos los dias que laboro.
            if (entryDate > payrollStart) daysWorked = (payrollEnd - entryDate).Days + 1;
            else  daysWorked = 15; 

            // Validaciones de seguridad para evitar días negativos o excesivos
            if (daysWorked < 0) daysWorked = 0;
            if (daysWorked > 15) daysWorked = 15;

            decimal monthlySalary   = salary.AmountInLocal;
            decimal BiweeklySalary  = monthlySalary / 2;
            decimal dailySalary     = monthlySalary / 30;

            decimal ProportionalBiweeklySalary = dailySalary * daysWorked;

            decimal Overtime = 0.0m;
            int NumberOfOvertime = 0;

            decimal Bonus = 0.0m;

            decimal  GrossSalary = Overtime + Bonus + ProportionalBiweeklySalary;

            var TaxInformation = await _unitOfWork.IncomeTaxAccrual.Entities
                .Where(income => income.CollaboratorId == collaborator.Id)
                .OrderByDescending(income => income.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var (BiweeklyInss, BiweeklyIr) = await CalculateIrToNextProcess(
                TaxInformation?.NumberOfFortnights ?? 24,
                TaxInformation?.SalaryEarned ?? 0.0m,
                TaxInformation?.AccumulatedIR ?? 0.0m,
                ProportionalBiweeklySalary,
                cancellationToken
            );

            var asssineds = await _unitOfWork.AssignedTravelExpenses.Entities
                .Where(assigned => assigned.CollaboratorId == collaborator.Id && assigned.EndDate == null)
                .Include(asssined => asssined.TypeIncome)
                .ToListAsync(cancellationToken);


            decimal Lodging = 0.0m;
            decimal Transport = 0.0m;
            decimal FoodTravelAllowance = 0.0m;

            decimal totalAssigned = 0.0m;

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
                        Lodging = current.AmountInLocalCurrency * 13;
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

            decimal TotalLegalDeductions = BiweeklyInss + BiweeklyIr;
            decimal TotalDeducctions = TotalLegalDeductions;

            FoodTravelAllowance *= 13; //220
            Transport *= 13; // 55
            Lodging *= 13;

            totalAssigned *= 13;;

            decimal TotalToPay = GrossSalary - BiweeklyInss - BiweeklyIr + totalAssigned;

            var payload = new OrdinaryPayroll()
            {
                Id = Guid.NewGuid(),
                CollaboratorId       = collaborator.Id,
                PayrollId            = payrollId,

                FoodTravelAllowance  = FoodTravelAllowance,
                TotalTravelExpenses  = totalAssigned,
                Lodging              = Lodging,
                TravelExpenses       = Transport,
                
                BiweeklySalary       = BiweeklySalary,
                
                Overtime             = Overtime,
                NumberOfOvertime     = NumberOfOvertime,
                Bonus                = Bonus,
                GrossSalary          = GrossSalary,

                Inss                 = BiweeklyInss,
                Ir                   = BiweeklyIr,
                TotalLegalDeductions = TotalLegalDeductions,

                DeductionsAdditionalData = JsonSerializer.Serialize(AdditionalDeducctions),

                TotalDeducctions     = TotalDeducctions,
                TotalToPay           = TotalToPay,
            };




            if (collaborator.IdentificationNumber == "0010404780003G")
            {
                
            }


            var PayrollRegistered = await _unitOfWork.OrdinaryPayrolls.RegisterCollaboratorInTheOrdinaryPayroll(payload);

            await _unitOfWork.SaveChangesAsync(cancellationToken);


            // var lastIncomeTaxAccrual = await _unitOfWork.IncomeTaxAccrual.Entities
            //     .Include(inc => inc.Collaborator)
            //     .Include(inc => inc.Payroll)
            //     .Where(inc => inc.CollaboratorId == collaborator.Id)
            //     .OrderByDescending(inc => inc.CreatedAt)
            //     .FirstOrDefaultAsync(cancellationToken);

            
            // int NumberOfFortnights;


            // if (lastIncomeTaxAccrual is null)
            // {
            //     NumberOfFortnights = 24;
            // }
            // else
            // {
            //     NumberOfFortnights = lastIncomeTaxAccrual.NumberOfFortnights - 1;
            // }

            // var IncomeTaxAccrualPayload = new IncomeTaxAccrual()
            // {
            //     Id = Guid.NewGuid(),
            //     AccumulatedIR = (lastIncomeTaxAccrual?.AccumulatedIR ?? 0.0m) + BiweeklyIr,
            //     SalaryEarned = (lastIncomeTaxAccrual?.SalaryEarned ?? 0.0m) + (GrossSalary - BiweeklyInss),
            //     CollaboratorId = collaborator.Id,
            //     PayrollId = PayrollRegistered.Id,
            //     NumberOfFortnights = NumberOfFortnights,
            //     RegisterDate = DateTime.Now
            // };


            // await _unitOfWork.IncomeTaxAccrual.RegisterIncomeTaxAccrual(IncomeTaxAccrualPayload);

        }
    }
}