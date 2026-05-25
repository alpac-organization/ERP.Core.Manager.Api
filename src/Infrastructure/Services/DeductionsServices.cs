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

    public class DeductionsServices(IUnitOfWork _unitOfWork, ILogger<CalculatorDeductions> _logger) : IDeductionsServices
    {
        public async Task ApplyDeductionTravelExpenses(Collaborator collaboratorInformation, Salary salaryInformation)
        {
            _logger.LogInformation("🚩Iniciando proceso de deducción de viaticos. por ausencia de dias para colaborador con cedula: {identification}", collaboratorInformation.IdentificationNumber);

            if (salaryInformation.SalaryType != SalaryType.Fixed)
            {
                _logger.LogInformation("Los colaboradores con salario variable o prestaciado todavia no puede establecer esta acción");
                return;
            }

            var payrollActive = await _unitOfWork.Payrolls.Entities
                .Where(pay => pay.Status == PayrollStatus.Progress)
                .Where(pay => pay.PayrollType == PayrollType.Ordinary)
                .Where(pay => pay.BranchId == collaboratorInformation.WorkingInformation.CompanyBranchId)
                .FirstOrDefaultAsync(default);

            if (payrollActive is null)
            {
                _logger.LogInformation("No se encuentra una nomina en progreso para continuar con este proceso");
                return;
            }

            DateTime payrollStartDate = payrollActive.StartDate.ToLocalTime().Date;
            DateTime payrollEndDate   = payrollActive.EndDate.ToLocalTime().Date;

            var permitApplications = await _unitOfWork.PermitApplications.Entities
                .Where(per => per.Status == PermitApplicationStatus.Approved)
                .Where(per =>
                    per.Type == PermitApplicationType.Vacation ||
                    per.Type == PermitApplicationType.MedicalAppointment
                )
                .Where(per => per.CollaboratorId == collaboratorInformation.Id)
                .ToListAsync();

            decimal totalAmountDays = 0;

            foreach (var permit in permitApplications)
            {
                DateTime permitStart = permit.StartDate!.Value.ToLocalTime().Date;
                DateTime permitEnd   = permit.EndDate!.Value.ToLocalTime().Date;

                // No intersecta
                if (permitEnd < payrollStartDate || permitStart > payrollEndDate)
                    continue;

                bool isFullyInsidePayroll =
                    permitStart >= payrollStartDate &&
                    permitEnd <= payrollEndDate;

                // CASO 1:
                // El permiso está completamente dentro de la nómina
                if (isFullyInsidePayroll)
                {
                    totalAmountDays += permit.AmountDays ?? 0;
                    continue;
                }

                // CASO 2:
                // Intersección parcial
                DateTime overlapStart =
                    permitStart > payrollStartDate
                        ? permitStart
                        : payrollStartDate;

                DateTime overlapEnd =
                    permitEnd < payrollEndDate
                        ? permitEnd
                        : payrollEndDate;

                decimal overlapDays =
                    (overlapEnd - overlapStart).Days + 1;

                totalAmountDays += overlapDays;
            }

            var ordinaryPayroll = await _unitOfWork.OrdinaryPayrolls.Entities
                .Where(ord => ord.CollaboratorId == collaboratorInformation.Id)
                .Where(ord => ord.PayrollId == payrollActive.Id)
                .FirstOrDefaultAsync(default);

            if (ordinaryPayroll is null)
            {
                _logger.LogInformation("No se encontro la información de nomina para el colaborador: {identification}", collaboratorInformation.IdentificationNumber);
                return;
            }

            var assignedTravelExpenses = await _unitOfWork.AssignedTravelExpenses.Entities
                .Where(assign => assign.CollaboratorId == collaboratorInformation.Id)
                .Where(assign => assign.EndDate == null)
                .Include(assign => assign.TypeIncome)
                .ToListAsync(default);

            decimal transport   = 0.0m;
            decimal feeding     = 0.0m;
            decimal lodging     = 0.0m;

            foreach (var assigne in assignedTravelExpenses)
            {
                if (assigne.TypeIncome.IncomeCode == "ALW_MEAL")
                {
                    feeding += assigne.AmountInLocalCurrency;
                }

                if (assigne.TypeIncome.IncomeCode == "ALW_HOUSING")
                {
                    lodging += assigne.AmountInLocalCurrency;
                }

                if (assigne.TypeIncome.IncomeCode == "ALW_TRANSPORT")
                {
                    transport += assigne.AmountInLocalCurrency;
                }
            }

            int totalDays = collaboratorInformation.DoesWorkSaturdays ? 13 : 11;

            //Calcular el total de viaticos que gana esta persona.
            
            decimal totalFeeding   = totalDays * feeding;
            decimal totalTransport = totalDays * transport;
            decimal totalLodging   = totalDays * lodging;
            decimal totalTravels   = totalFeeding + totalTransport + totalLodging;

            //Ahora que sabemos el total de recibe deduscamos los dias que no viene esa persona.

            int totalDaysToDiscount = (int) Math.Floor(totalAmountDays);

            decimal totalProporcionalFeeding    = feeding * totalDaysToDiscount;
            decimal totalProporcionalTransport  = transport * totalDaysToDiscount;
            decimal totalProporcionalLodging    = lodging * totalDaysToDiscount;

            decimal totalProporcionalTravels  = totalProporcionalFeeding + totalProporcionalTransport + totalProporcionalLodging;

            ordinaryPayroll.Transport = totalTransport - totalProporcionalTransport;
            ordinaryPayroll.Feeding = totalFeeding - totalProporcionalFeeding;
            ordinaryPayroll.Lodging = totalLodging - totalProporcionalLodging;

            decimal totalToPay = ordinaryPayroll.TotalIncome - ordinaryPayroll.TotalLegalDeductions - ordinaryPayroll.TotalDeducctions + (totalTravels - totalProporcionalTravels);

            //Actualizar la reporteria actual de la nomina.

            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayroll);
            await _unitOfWork.SaveChangesAsync(default);

            _logger.LogInformation("✅ Deducción de viáticos aplicada correctamente. Total días: {Days}", totalAmountDays);
        }


        public async Task ApplyDeductionLateArrivals(Collaborator collaboratorInformation, Salary salaryInformation, decimal totalMinutes, Guid payrollId)
        {
            #region Iniciando calculo de deducción

            //Calculo de valor por horas extras.
            decimal DailySalary   = salaryInformation.AmountInLocal / 30;
            decimal HourlyWage    = DailySalary / 8;
            decimal PerMinuteWage = HourlyWage / 60;
                    
            decimal TotalDeductionToLateArrivals = totalMinutes * PerMinuteWage;

            _logger.LogInformation("Actualizando nomina para colaborador con cedula: {identification}", collaboratorInformation.IdentificationNumber);

            //Consultar la nomina activa actualmente

            var ordinaryPayroll = await _unitOfWork.OrdinaryPayrolls.Entities
                .Where(col => col.PayrollId == payrollId)
                .Where(col => col.CollaboratorId == collaboratorInformation.Id)
                .Include(col => col.Payroll)
                .FirstOrDefaultAsync(default);

            if (ordinaryPayroll is null)
            {
                _logger.LogInformation("No se encontro registro de nomina de este colaborador => {identificacion}", collaboratorInformation.IdentificationNumber);
                return;
            }

            #region Proceso deducción

            var deductions =
                JsonSerializer.Deserialize<DeductionsAdditionalData>(
                    ordinaryPayroll.DeductionsAdditionalData
                ) ?? new DeductionsAdditionalData();

            deductions.LateArrivals = TotalDeductionToLateArrivals;

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


            deductions.LateArrivalsInMinutes = totalMinutes;
            decimal total = ordinaryPayroll.TotalIncome - ordinaryPayroll.TotalLegalDeductions - totalDeductions + ordinaryPayroll.TotalTravelExpenses;

            ordinaryPayroll.TotalToPay = total;
            ordinaryPayroll.TotalDeducctions = ordinaryPayroll.TotalLegalDeductions + totalDeductions;

            ordinaryPayroll.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);

            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayroll);

            #endregion Proceso deducción

            _logger.LogInformation("Se finaliza el proceso de actualización de datos de nomina");

            #region Registro de deducciones
            var deduction = await _unitOfWork.Deductions.RegisterDeduction(new()
            {
                Id                   = Guid.NewGuid(),
                Currency             = Currency.NIO,
                Status               = DeductionStatus.Completed,
                Type                 = DeductionType.LateArrivals,
                CollaboratorId       = collaboratorInformation.Id,
                Description          = "Llegadas tardes",
                Amount               = totalMinutes,              
                TotalAmount          = TotalDeductionToLateArrivals,
                TotalAmountInDollars = TotalDeductionToLateArrivals / 36.6242m
            });

            await _unitOfWork.DeductionPaymentHistories.RegisterDeductionPaymentHistory(new()
            {
                Currency            = Currency.NIO,
                Status              = DeductionPaymentStatus.Paid,
                Origin              = SourceDeductionPayment.Payroll,
                DeductionId         = deduction.Id,
                PayrollId           = ordinaryPayroll.PayrollId,           
                AmountPaid          = TotalDeductionToLateArrivals,
                AmountPaidInDollars = TotalDeductionToLateArrivals,
                PaymentDate         = ordinaryPayroll.Payroll.EndDate
            });

            #endregion

            #endregion
        }

        public async Task ApplyDeductionPurisima(Collaborator collaboratorInformation, decimal fortnightlyAmount, Guid payrollId)
        {
            var ordinaryPayroll = await _unitOfWork.OrdinaryPayrolls.Entities
                .Where(ord => ord.PayrollId == payrollId)
                .Where(ord => ord.CollaboratorId == collaboratorInformation.Id)
                .Include(or => or.Payroll)
                .FirstOrDefaultAsync(default);

            if (ordinaryPayroll is null)
            {
                _logger.LogInformation("No se encontro registro de nomina de este colaborador => {identificacion}", collaboratorInformation.IdentificationNumber);
                return;
            }

            var deductions =
                JsonSerializer.Deserialize<DeductionsAdditionalData>(
                    ordinaryPayroll.DeductionsAdditionalData
                ) ?? new DeductionsAdditionalData();

            deductions.Purisima = fortnightlyAmount;

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

            decimal total = ordinaryPayroll.GrossSalary - ordinaryPayroll.TotalLegalDeductions - totalDeductions + ordinaryPayroll.TotalTravelExpenses;

            ordinaryPayroll.TotalToPay = total;
            ordinaryPayroll.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);

            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayroll);

            var deduction = await _unitOfWork.Deductions.RegisterDeduction(new()
            {   Id                          = Guid.NewGuid(),
                Currency                    = Currency.NIO,
                Type                        = DeductionType.Purisima,
                Status                      = DeductionStatus.Progress,
                Description                 = "Aportación de purisima",
                CollaboratorId              = collaboratorInformation.Id,
                FortnightlyAmount           = fortnightlyAmount,
                FortnightlyAmountInDollars  = fortnightlyAmount / 36.6243m,
                AmountPaid                  = fortnightlyAmount,
                AmountPaidInDollars         = fortnightlyAmount,
                TotalAmount                 = fortnightlyAmount,
                TotalAmountInDollars        = fortnightlyAmount / 36.6243m,
            });

            await _unitOfWork.DeductionPaymentHistories.RegisterDeductionPaymentHistory(new()
            {
                Currency            = Currency.NIO,
                Status              = DeductionPaymentStatus.Paid,
                Origin              = SourceDeductionPayment.Payroll,
                PayrollId           = payrollId,
                DeductionId         = deduction.Id,
                AmountPaid          = fortnightlyAmount,
                AmountPaidInDollars = fortnightlyAmount / 36.6243m,
                PaymentDate         = ordinaryPayroll.Payroll.EndDate
            });
        }
    }
    
    #pragma warning restore CA1873
}