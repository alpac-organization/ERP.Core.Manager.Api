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
                PaymentDate         = ordinaryPayroll.Payroll.EndDate ?? DateTime.Now
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
                PaymentDate         = ordinaryPayroll.Payroll.EndDate ?? DateTime.Now
            });
        }
    }
    
    #pragma warning restore CA1873
}