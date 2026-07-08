using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;


namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    public class DeductionsServices(IUnitOfWork _unitOfWork, ILogger<CalculatorDeductions> _logger) : IDeductionsServices
    {

        public async Task<bool> ApplySansion(Collaborator collaboratorInformation, int amountDays, Guid payrollId)
        {
            var deductionPayload = new Deduction()
            {
                Id              = Guid.NewGuid(),
                CollaboratorId  = collaboratorInformation.Id,
                Description     = "Sanción por inasistencia",
                Currency        = Currency.NIO,
                Type            = DeductionType.Sanction,
                Status          = DeductionStatus.Progress,
                Amount          = amountDays,
            };

            var exchangeRate = await _unitOfWork.ValidityDeductions.Entities
                .Where(val => val.Status)
                .Where(val => val.EndDate == null)
                .Where(val => val.Type == TaxType.ExchangeRate)
                .FirstOrDefaultAsync(default);

            if (exchangeRate is null)
            {
                _logger.LogWarning("No se encontró un tipo de cambio activo en la configuración.");
                return false;
            }

            var salaryInformation = await _unitOfWork.Salaries.Entities
                .Where(salary => salary.EndDate == null)
                .Where(salary => salary.SalaryType == SalaryType.Fixed)
                .Where(salary => salary.CollaboratorId == collaboratorInformation.Id)
                .FirstOrDefaultAsync(default);

            if (salaryInformation is null)
            {
                _logger.LogWarning("No se pudo obtener la información salarial del colaborador con cedula: {identification}", collaboratorInformation.IdentificationNumber);
                return false;
            }

            var ordinaryPayroll = await _unitOfWork.OrdinaryPayrolls.Entities
                .Where(ord => ord.PayrollId == payrollId)
                .Include(or => or.Payroll)
                .Where(ord => ord.CollaboratorId == collaboratorInformation.Id)
                .Include(or => or.Payroll)
                .FirstOrDefaultAsync(default);

            if (ordinaryPayroll is null)
            {
                _logger.LogInformation("No se encontro registro de nomina de este colaborador => {identificacion}", collaboratorInformation.IdentificationNumber);
                return false;
            }

            var inssAccountingInformation = await _unitOfWork.InssAccountingInformation.Entities
                .Where(acc => acc.CollaboratorId == collaboratorInformation.Id)
                .Where(acc => acc.PayrollId == payrollId)
                .FirstOrDefaultAsync(default);

            if (inssAccountingInformation is null)
            {
                _logger.LogWarning("No se encontró información contable del INSS para el colaborador: {identification}", collaboratorInformation.IdentificationNumber);
                return false;
            }


            #region Actualizar reportes de ir y nomina


            #endregion

            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayroll);
            await _unitOfWork.Deductions.RegisterDeduction(deductionPayload);
            return true;
        }



        //✅Deducción de viaticos por inasistencia
        public async Task ApplyDeductionTravelExpenses(Collaborator collaboratorInformation, Salary salaryInformation, Guid payrollId)
        {
            _logger.LogInformation("🚩Iniciando proceso de deducción de viaticos. por ausencia de dias para colaborador con cedula: {identification}", collaboratorInformation.IdentificationNumber);

            if (salaryInformation.SalaryType != SalaryType.Fixed)
            {
                _logger.LogInformation("Los colaboradores con salario variable o prestaciado todavia no puede establecer esta acción");
                return;
            }

            var payrollActive = await _unitOfWork.Payrolls.Entities
                .Where(pay => pay.Id == payrollId)
                .Where(pay => pay.Status == PayrollStatus.Progress)
                .Where(pay => pay.PayrollType == PayrollType.Ordinary)
                .Where(pay => pay.BranchId == collaboratorInformation.WorkingInformation.CompanyBranchId)
                .FirstOrDefaultAsync(default);

            if (payrollActive is null)
            {
                _logger.LogInformation("No se encuentra una nomina en progreso para continuar con este proceso");
                return;
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

            var permitApplications = await _unitOfWork.PermitApplications.Entities
                .Where(permit => permit.Status == PermitApplicationStatus.Approved)
                .Where(permit => permit.CollaboratorId == collaboratorInformation.Id)
                .Where(permit => permit.Type == PermitApplicationType.Vacation || permit.Type == PermitApplicationType.MedicalAppointment)
                .Include(permit => permit.Collaborator)
                    .ThenInclude(col => col.WorkingInformation)
                .ToListAsync(default);

            var holidays = await _unitOfWork.Holidays.Entities
                .Where(day => day.IsGlobal)
                .ToListAsync(default);

            int totalDaysDefault = 0;
            int totalDaysToDiscount = 0;
            decimal totalDaysResult = 0;

            #region Calculo de dias permitidos a tener viaticos en la quincena.

            for (DateOnly date = payrollActive.StartDate; date <= payrollActive.EndDate; date = date.AddDays(1))
            {
                bool isHoliday = holidays.Any(holiday =>
                    holiday.Day == date.Day &&
                    holiday.Month == date.Month &&
                    (
                        holiday.IsGlobal ||
                        holiday.BranchId == collaboratorInformation.WorkingInformation.CompanyBranchId
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

                if (!collaboratorInformation.DoesWorkSaturdays && date.DayOfWeek == DayOfWeek.Saturday)
                {
                    continue;
                }

                totalDaysDefault++;
            }

            #endregion

            #region Calculo de dias de incosistencia. quitar a la cantidad total esos dias de viaticos y sumarlos para restarlo al disponibles

            foreach (var permit in permitApplications)
            {
                DateOnly permitStartDate = permit.StartDate;
                DateOnly permitEndDate = permit.EndDate;

                for (DateOnly date = permitStartDate; date <= permitEndDate; date = date.AddDays(1))
                {
                    if (date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        continue;
                    }

                    if (!collaboratorInformation.DoesWorkSaturdays && date.DayOfWeek == DayOfWeek.Saturday)
                    {
                        continue;
                    }

                    bool isHoliday = holidays.Any(holiday => holiday.Day == date.Day && holiday.Month == date.Month &&
                        (
                            holiday.IsGlobal ||
                            holiday.BranchId == collaboratorInformation.WorkingInformation.CompanyBranchId
                        )
                    );

                    if (isHoliday)
                    {
                        continue;
                    }

                    if (
                        collaboratorInformation.DoesWorkSaturdays &&
                        date.DayOfWeek == DayOfWeek.Saturday &&
                        permit.IsWithRangeDate is false &&
                        permit.AmountDays == 0.5m
                    )
                    {
                        totalDaysToDiscount++;
                        continue;
                    }

                    totalDaysToDiscount++;
                }
            }

            #endregion

            int result = (int)Math.Floor(totalDaysResult);
            totalDaysToDiscount += result;

            var assignedTravelExpenses = await _unitOfWork.AssignedTravelExpenses.Entities
                .Where(assign => assign.CollaboratorId == collaboratorInformation.Id)
                .Where(assign => assign.EndDate == null)
                .Include(assign => assign.TypeIncome)
                .ToListAsync(default);

            decimal transport = 0.0m;
            decimal feeding = 0.0m;
            decimal lodging = 0.0m;

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

            // //Calcular el total de viaticos que gana esta persona.
            decimal totalFeeding = totalDaysDefault * feeding;
            decimal totalTransport = totalDaysDefault * transport;
            decimal totalLodging = totalDaysDefault * lodging;
            decimal totalTravels = totalFeeding + totalTransport + totalLodging;

            // //Ahora que sabemos el total de recibe deduscamos los dias que no viene esa persona.

            decimal totalProporcionalFeeding = feeding * totalDaysToDiscount;
            decimal totalProporcionalTransport = transport * totalDaysToDiscount;
            decimal totalProporcionalLodging = lodging * totalDaysToDiscount;

            decimal totalProporcionalTravels = totalProporcionalFeeding + totalProporcionalTransport + totalProporcionalLodging;

            ordinaryPayroll.Transport = totalTransport - totalProporcionalTransport;
            ordinaryPayroll.Feeding = totalFeeding - totalProporcionalFeeding;
            ordinaryPayroll.Lodging = totalLodging - totalProporcionalLodging;

            ordinaryPayroll.TotalTravelExpenses = totalTravels - totalProporcionalTravels;

            decimal totalToPay = ordinaryPayroll.TotalIncome - ordinaryPayroll.TotalDeducctions + ordinaryPayroll.TotalTravelExpenses;
            ordinaryPayroll.TotalToPay = totalToPay;

            //Actualizar la reporteria actual de la nomina.

            // your code here.
            var recordInformation = await _unitOfWork.RecordsTravelExpensePayments.Entities
                .Where(history => history.CollaboratorId == collaboratorInformation.Id)
                .Where(history => history.PayrollId == payrollId)
                .FirstOrDefaultAsync(default);

            if (recordInformation is null)
            {
                _logger.LogInformation("No se encontro el informe de pago de viaticos");
                return;
            }

            //Actualizar la reporteria actual de la nomina.
            recordInformation.Lodging = totalLodging - totalProporcionalLodging;
            recordInformation.Transport = totalTransport - totalProporcionalTransport;
            recordInformation.Feeding = totalFeeding - totalProporcionalFeeding;

            await _unitOfWork.RecordsTravelExpensePayments.UpdateAsync(recordInformation);

            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayroll);
            await _unitOfWork.SaveChangesAsync(default);

            _logger.LogInformation("✅ Deducción de viáticos aplicada correctamente. Total días: {Days}", totalDaysToDiscount);
        }

        //✅Deducción por llegadas tardes. Listo
        public async Task ApplyDeductionLateArrivals(Collaborator collaboratorInformation, Salary salaryInformation, decimal totalMinutes, Guid payrollId)
        {
            #region Iniciando calculo de deducción

            //Calculo de valor por horas extras.
            decimal DailySalary = salaryInformation.AmountInLocal / 30;
            decimal HourlyWage = DailySalary / 8;
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
                Id = Guid.NewGuid(),
                Currency = Currency.NIO,
                Status = DeductionStatus.Completed,
                Type = DeductionType.LateArrivals,
                CollaboratorId = collaboratorInformation.Id,
                Description = "Llegadas tardes",
                Amount = totalMinutes,
                TotalAmount = TotalDeductionToLateArrivals,
                TotalAmountInDollars = TotalDeductionToLateArrivals / 36.6242m
            });

            await _unitOfWork.DeductionPaymentHistories.RegisterDeductionPaymentHistory(new()
            {
                Currency = Currency.NIO,
                Status = DeductionPaymentStatus.Paid,
                Origin = SourceDeductionPayment.Payroll,
                DeductionId = deduction.Id,
                PayrollId = ordinaryPayroll.PayrollId,
                AmountPaid = TotalDeductionToLateArrivals,
                AmountPaidInDollars = TotalDeductionToLateArrivals,
                PaymentDate = DateTime.Now
            });

            #endregion

            #endregion
        }

        //✅Deducción por purisima. Listo
        public async Task ApplyDeductionPurisima(Collaborator collaboratorInformation, decimal amount, Guid payrollId, int numberFortnights)
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

            decimal fortnightlyAmount = amount / numberFortnights;

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

            decimal total = ordinaryPayroll.TotalIncome - ordinaryPayroll.TotalLegalDeductions - totalDeductions + ordinaryPayroll.TotalTravelExpenses;

            ordinaryPayroll.TotalToPay = total;
            ordinaryPayroll.TotalDeducctions = ordinaryPayroll.TotalLegalDeductions + totalDeductions;
            ordinaryPayroll.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);

            await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayroll);

            var deduction = await _unitOfWork.Deductions.RegisterDeduction(new()
            {
                Id = Guid.NewGuid(),
                Currency = Currency.NIO,
                Type = DeductionType.Purisima,
                Status = DeductionStatus.Progress,
                Description = "Aportación de purisima",
                CollaboratorId = collaboratorInformation.Id,

                FortnightlyAmount = fortnightlyAmount,
                FortnightlyAmountInDollars = fortnightlyAmount / 36.6243m,

                AmountPaid = 0.0m,
                AmountPaidInDollars = 0.0m,

                TotalBalance = amount,
                TotalBalanceInDollars = amount / 36.6243m,

                NumberFortnights = numberFortnights,
                NumberFortnightsPaid = 0,

                TotalAmount = fortnightlyAmount,
                TotalAmountInDollars = fortnightlyAmount / 36.6243m,
            });

            await _unitOfWork.DeductionPaymentHistories.RegisterDeductionPaymentHistory(new()
            {
                Currency = Currency.NIO,
                Status = DeductionPaymentStatus.Pending,
                Origin = SourceDeductionPayment.Payroll,
                PayrollId = payrollId,
                DeductionId = deduction.Id,

                AmountPaid = fortnightlyAmount,
                AmountPaidInDollars = fortnightlyAmount / 36.6243m,
                PaymentDate = DateTime.Now
            });
        }

        //✅Deducción por prestamos. Listo
        public async Task ApplyDeductionLoans(Collaborator collaboratorInformation, decimal amount, Guid payrollId, int numberFortnights, Currency currency, string description = "Registro de préstamo")
        {

            const decimal exchangeRate = 36.6243m;
            decimal fortnightlyAmount = amount / numberFortnights;
            var deductionId = Guid.NewGuid();

            var loanActive = await _unitOfWork.Deductions.Entities
                .Where(ded => ded.Type == DeductionType.Loans)
                .Where(ded => ded.Status == DeductionStatus.Progress)
                .Where(ded => ded.CollaboratorId == collaboratorInformation.Id)
                .ToListAsync(default);

            if (loanActive.Count > 0)
            {
                // Manejar el caso donde ya existe un préstamo activo
                await _unitOfWork.Deductions.RegisterDeduction(new()
                {
                    Id = deductionId,
                    Currency = currency,
                    Type = DeductionType.Loans,
                    Status = DeductionStatus.Pending,
                    Description = description,
                    CollaboratorId = collaboratorInformation.Id,

                    FortnightlyAmount = currency == Currency.NIO
                        ? fortnightlyAmount
                        : fortnightlyAmount * exchangeRate,

                    FortnightlyAmountInDollars = currency == Currency.USD
                        ? fortnightlyAmount
                        : fortnightlyAmount / exchangeRate,

                    // Pagado
                    AmountPaid = 0.0m,
                    AmountPaidInDollars = 0.0m,

                    // Saldo pendiente
                    TotalBalance = currency == Currency.NIO
                        ? amount
                        : amount * exchangeRate,

                    TotalBalanceInDollars = currency == Currency.USD
                        ? amount
                        : amount / exchangeRate,

                    NumberFortnights = numberFortnights,
                    NumberFortnightsPaid = 0,

                    // Monto total del préstamo
                    TotalAmount = currency == Currency.NIO
                        ? amount
                        : amount * exchangeRate,

                    TotalAmountInDollars = currency == Currency.USD
                        ? amount
                        : amount / exchangeRate,
                });
            }
            else
            {
                await _unitOfWork.Deductions.RegisterDeduction(new()
                {
                    Id = deductionId,
                    Currency = currency,
                    Type = DeductionType.Loans,
                    Status = DeductionStatus.Progress,
                    Description = description,
                    CollaboratorId = collaboratorInformation.Id,

                    FortnightlyAmount = currency == Currency.NIO
                        ? fortnightlyAmount
                        : fortnightlyAmount * exchangeRate,

                    FortnightlyAmountInDollars = currency == Currency.USD
                        ? fortnightlyAmount
                        : fortnightlyAmount / exchangeRate,

                    // Pagado
                    AmountPaid = 0.0m,
                    AmountPaidInDollars = 0.0m,

                    // Saldo pendiente
                    TotalBalance = currency == Currency.NIO
                        ? amount
                        : amount * exchangeRate,

                    TotalBalanceInDollars = currency == Currency.USD
                        ? amount
                        : amount / exchangeRate,

                    NumberFortnights = numberFortnights,
                    NumberFortnightsPaid = 0,

                    // Monto total del préstamo
                    TotalAmount = currency == Currency.NIO
                        ? amount
                        : amount * exchangeRate,

                    TotalAmountInDollars = currency == Currency.USD
                        ? amount
                        : amount / exchangeRate,
                });

                //Aplicar la deducción del préstamo en la nómina actual del colaborador.
                var ordinaryPayroll = await _unitOfWork.OrdinaryPayrolls.Entities
                    .Where(ord => ord.PayrollId == payrollId)
                    .Where(ord => ord.CollaboratorId == collaboratorInformation.Id)
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

                deductions.Loans = currency == Currency.NIO
                    ? fortnightlyAmount
                    : fortnightlyAmount * exchangeRate;

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


                decimal total = ordinaryPayroll.TotalIncome - ordinaryPayroll.TotalLegalDeductions - totalDeductions + ordinaryPayroll.TotalTravelExpenses;

                ordinaryPayroll.TotalToPay = total;
                ordinaryPayroll.TotalDeducctions = ordinaryPayroll.TotalLegalDeductions + totalDeductions;

                ordinaryPayroll.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);

                await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayroll);


                await _unitOfWork.DeductionPaymentHistories.RegisterDeductionPaymentHistory(new()
                {
                    Currency = currency,
                    Status = DeductionPaymentStatus.Pending,
                    Origin = SourceDeductionPayment.Payroll,
                    PayrollId = payrollId,
                    DeductionId = deductionId,

                    AmountPaid = currency == Currency.NIO
                        ? fortnightlyAmount
                        : fortnightlyAmount * exchangeRate,

                    AmountPaidInDollars = currency == Currency.USD
                        ? fortnightlyAmount
                        : fortnightlyAmount / exchangeRate,

                    PaymentDate = DateTime.UtcNow,
                });
            }
        }

        public async Task ApplyJudicialGarnishment(Collaborator collaboratorInformation, decimal totalAmount, int percentage, Currency currency, string description, Guid payrollId)
        {
            var exchangeRateEntity = await _unitOfWork.ValidityDeductions.Entities
               .Where(val => val.Status)
               .Where(val => val.EndDate == null)
               .Where(val => val.Type == TaxType.ExchangeRate)
               .FirstOrDefaultAsync(default);


            if (exchangeRateEntity is null)
            {
                _logger.LogWarning("No se encontró un tipo de cambio activo en la configuración.");
                return;
            }

            decimal exchangeRate = exchangeRateEntity.Value;
            var deductionId = Guid.NewGuid();

            var garnishmentActive = await _unitOfWork.Deductions.Entities
                .Where(ded => ded.Type == DeductionType.JudicialSeizures)
                .Where(ded => ded.Status == DeductionStatus.Progress)
                .Where(ded => ded.CollaboratorId == collaboratorInformation.Id)
                .AnyAsync(default);

            var deductionStatus = garnishmentActive ? DeductionStatus.Pending : DeductionStatus.Progress;

            await _unitOfWork.Deductions.RegisterDeduction(new()
            {
                Id = deductionId,
                Currency = currency,
                Type = DeductionType.JudicialSeizures,
                Status = deductionStatus,
                Description = description,
                CollaboratorId = collaboratorInformation.Id,

                Percentage = percentage,
                FortnightlyAmount = 0.0m,
                FortnightlyAmountInDollars = 0.0m,
                AmountPaid = 0.0m,
                AmountPaidInDollars = 0.0m,
                TotalBalance = currency == Currency.NIO ? totalAmount : totalAmount * exchangeRate,
                TotalBalanceInDollars = currency == Currency.USD ? totalAmount : totalAmount / exchangeRate,
                TotalAmount = currency == Currency.NIO ? totalAmount : totalAmount * exchangeRate,
                TotalAmountInDollars = currency == Currency.USD ? totalAmount : totalAmount / exchangeRate,
            });
            var ordinaryPayroll = await _unitOfWork.OrdinaryPayrolls.Entities
                .Where(ord => ord.PayrollId == payrollId && ord.CollaboratorId == collaboratorInformation.Id)
                .FirstOrDefaultAsync(default);

            if (ordinaryPayroll is not null && deductionStatus == DeductionStatus.Progress)
            {
                decimal baseAmount = ordinaryPayroll.TotalIncome - ordinaryPayroll.TotalLegalDeductions;
                decimal amountToDeduct = Math.Round(baseAmount * (percentage / 100m), 2, MidpointRounding.AwayFromZero);

                decimal maxBalance = currency == Currency.NIO ? totalAmount : totalAmount * exchangeRate;
                if (amountToDeduct > maxBalance)
                {
                    amountToDeduct = maxBalance;
                }

                var deductions = JsonSerializer.Deserialize<DeductionsAdditionalData>(ordinaryPayroll.DeductionsAdditionalData ?? "{}") ?? new DeductionsAdditionalData();
                deductions.JudicialSeizures = amountToDeduct;

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

                decimal total = ordinaryPayroll.TotalIncome - ordinaryPayroll.TotalLegalDeductions - totalDeductions + ordinaryPayroll.TotalTravelExpenses;
                ordinaryPayroll.TotalToPay = total;
                ordinaryPayroll.TotalDeducctions = ordinaryPayroll.TotalLegalDeductions + totalDeductions;
                ordinaryPayroll.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);

                await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayroll);

                await _unitOfWork.DeductionPaymentHistories.RegisterDeductionPaymentHistory(new()
                {
                    DeductionId = deductionId,
                    AmountPaid = amountToDeduct,
                    AmountPaidInDollars = amountToDeduct / exchangeRate,
                    Status = DeductionPaymentStatus.Pending,
                    Origin = SourceDeductionPayment.Payroll,
                    Currency = currency,
                    PayrollId = payrollId,
                    PaymentDate = DateTime.Now
                });
            }
        }
    }
}