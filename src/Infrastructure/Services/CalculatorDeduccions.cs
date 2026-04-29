using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Database.Domain.Entities.Payrolls;
using System.Text.Json.Nodes;
using System.Text.Json;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

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
        public async Task<IrCalculationResult> CalculateIrToNextProcess(int NFortnight, decimal AccumulatedAccrued, decimal AccumulatedIR, decimal GrossSalary, CancellationToken cancellationToken)
        {
            var biweeklyInss = await CalculateInss(GrossSalary, cancellationToken);

            //Salario quincenal libre de inss.
            decimal netSalary = GrossSalary - biweeklyInss;
            decimal AnnualSalary = netSalary * NFortnight;

            decimal totalAnnualSalary = AnnualSalary + AccumulatedAccrued;
            decimal AnnualIr;

            //Agregar regla del ir
            decimal BaseTax;
            decimal AnnualExpectationIR = 0.0m;
            decimal IrBiweekly = 0.0m;


            if (totalAnnualSalary <= 100000)
                AnnualIr = 0;
            else if (AnnualSalary <= 200000)
            {
                BaseTax = 0;
                AnnualIr = ((AnnualSalary - 100000) * 0.15m);
                AnnualExpectationIR = AnnualIr + BaseTax;

                IrBiweekly = (AnnualExpectationIR - AccumulatedIR) / NFortnight;
            }
            else if (AnnualSalary <= 350000)
            {
                BaseTax = 15000.00m;
                AnnualIr = ((AnnualSalary - 200000) * 0.20m);
                AnnualExpectationIR = AnnualIr + BaseTax;

                IrBiweekly = (AnnualExpectationIR - AccumulatedIR) / NFortnight;
            }
            else if (AnnualSalary <= 500000)
            {
                BaseTax = 45000.00m;
                AnnualIr = ((AnnualSalary - 350000) * 0.25m);
                AnnualExpectationIR = AnnualIr + BaseTax;

                IrBiweekly = (AnnualExpectationIR - AccumulatedIR) / NFortnight;
            }
            else
            {
                BaseTax = 82500.00m;
                AnnualIr = ((AnnualSalary - 500000) * 0.30m);

                AnnualExpectationIR = AnnualIr + BaseTax;
                IrBiweekly = (AnnualExpectationIR - AccumulatedIR) / NFortnight;
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

            //Este inss es proporcional a los dias laborados
            decimal InssBiweekly = await CalculateInss(GrossSalary, cancellationToken);
            decimal IrBiweekly  = await CalculateIr(GrossSalary, daysWorked, cancellationToken);
            
            decimal TotalToPay = GrossSalary - InssBiweekly - IrBiweekly;

            decimal TotalLegalDeductions = InssBiweekly + IrBiweekly;

            //Hacer un proceso de verificación de deducciones.
            
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

            decimal TotalDeducctions = TotalLegalDeductions;

            var payload = new OrdinaryPayroll()
            {
                CollaboratorId       = collaborator.Id,
                PayrollId            = payrollId,
                
                BiweeklySalary       = BiweeklySalary,
                
                Overtime             = Overtime,
                NumberOfOvertime     = NumberOfOvertime,
                Bonus                = Bonus,
                GrossSalary          = GrossSalary,

                Inss                 = InssBiweekly,
                Ir                   = IrBiweekly,
                TotalLegalDeductions = TotalLegalDeductions,

                DeductionsAdditionalData = JsonSerializer.Serialize(AdditionalDeducctions),

                TotalDeducctions     = TotalDeducctions,
                TotalToPay           = TotalToPay,
            };

            await _unitOfWork.OrdinaryPayrolls.RegisterCollaboratorInTheOrdinaryPayroll(payload);
        }
    }
}