using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Handlers
{
    public class ClosePayrollProcessHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<ClosePayrollProcessHandler> _logger) : AlpacBaseHandler<ClosePayrollProcessCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(ClosePayrollProcessCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            if (access.Role!.RoleType != RoleType.Administrator)
            {
                return _errorManager.ThrowBadRequest<bool>("Solo los administradores pueden cerrar el proceso de nomina", "ERP:001");
            }

            var branch = await _unitOfWork.Branches.Entities
                .Where(branch => branch.Id == request.BranchId && branch.CompanyId == request.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (branch is null)
            {
                return _errorManager.ThrowBadRequest<bool>("La sucursal seleccionada no estas asociado a este compañia", "ERP:BrachNotFound");
            }

            #region Verificar estado de la nomina

            var payroll = await _unitOfWork.Payrolls.Entities
                .Where(pay =>
                    pay.BranchId == request.BranchId && pay.PayrollType == request.PayrollType
                )
                .Where(pay =>
                    pay.Id == request.PayrollId && pay.Status == PayrollStatus.Progress
                )
                .Include(pay => pay.OrdinaryPayrolls)
                    .ThenInclude(or => or.Collaborator)
                .FirstOrDefaultAsync(cancellationToken);

            if (payroll is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Esta nomina no se encuentra en curso o no existe", "ERP:02");
            }

            #endregion

            payroll.Status = PayrollStatus.Closed;
            await _unitOfWork.Payrolls.UpdateAsync(payroll);


            //permisos colgados de una payroll cerrada.
            var pendingPermits = await _unitOfWork.PermitApplications.Entities
                .Where(permit => permit.PayrolId == request.PayrollId && permit.Status == PermitApplicationStatus.Pending)
                .ToListAsync(cancellationToken);

            foreach (var permit in pendingPermits)
            {
                //actualizando su estado a cancelled
                permit.Status = PermitApplicationStatus.Cancelled;
                await _unitOfWork.PermitApplications.UpdateAsync(permit);
            }

            var registers = payroll.OrdinaryPayrolls;
            //Realizar todo aquel, registro de deducciones y pagos realizados del colaborador
            foreach (var collaborator in registers)
            {
                //Verificar si el colaborador se encuentra despedido.

                if (collaborator.Collaborator.HasBeenFired)
                {
                    //Este colaborador has sido despedido ya no continua en el progreso de nomina
                    _logger.LogInformation("El colaborador con identification: {identification}, has sido dado de baja", collaborator.Collaborator.IdentificationNumber);
                    continue;
                }


                var deductionsActive = await _unitOfWork.Deductions.Entities
                    .Where(deduction => deduction.CollaboratorId == collaborator.CollaboratorId)
                    .Where(deduction => deduction.Status == DeductionStatus.Progress)
                    .ToListAsync(cancellationToken);

                foreach (var deduction in deductionsActive)
                {
                    var payment = await _unitOfWork.DeductionPaymentHistories.Entities
                        .Where(paid => paid.DeductionId == deduction.Id)
                        .Where(paid => paid.Status == DeductionPaymentStatus.Pending)
                        .Include(paid => paid.Deduction)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (payment is null)
                    {
                        _logger.LogInformation("No cuenta con pagos pendientes, ha cancelado!");
                        continue;
                    }

                    // decimal amountInLocal = deduction.FortnightlyAmount ?? 0;
                    // decimal amountInDollars = deduction.FortnightlyAmountInDollars ?? 0;
                    decimal amountInLocal = payment.AmountPaid;
                    decimal amountInDollars = payment.AmountPaidInDollars;

                    deduction.AmountPaid += amountInLocal;
                    deduction.AmountPaidInDollars += amountInDollars;
                    deduction.NumberFortnightsPaid += 1;

                    deduction.TotalBalance -= amountInLocal;
                    deduction.TotalBalanceInDollars -= amountInDollars;

                    if (deduction.TotalBalance <= 0 && deduction.TotalBalanceInDollars <= 0)
                    {
                        deduction.Status = DeductionStatus.Completed;
                        await _unitOfWork.Deductions.UpdateAsync(deduction);

                        var nextGarnishment = await _unitOfWork.Deductions.Entities
                            .Where(d => d.CollaboratorId == deduction.CollaboratorId)
                            .Where(d => d.Type == DeductionType.JudicialSeizures)
                            .Where(d => d.Status == DeductionStatus.Pending)
                            .OrderBy(d => d.CreatedAt)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (nextGarnishment is not null)
                        {
                            nextGarnishment.Status = DeductionStatus.Progress;
                            await _unitOfWork.Deductions.UpdateAsync(nextGarnishment);
                        }
                    }

                    payment.Status = DeductionPaymentStatus.Paid;
                    await _unitOfWork.DeductionPaymentHistories.UpdateAsync(payment);

                    await _unitOfWork.Deductions.UpdateAsync(deduction);
                }

                //Acumulado de vacaciones
                const decimal valueVacations = 1.25m;
                int daysWithSubsidy = 0;

                var subsidy = await _unitOfWork.Subsidies.Entities
                    .Where(sub => sub.CollaboratorId == collaborator.CollaboratorId)
                    .Where(sub => sub.PayrollId == collaborator.PayrollId)
                    .Include(sub => sub.TypesSubsidy)
                    .Where(sub => sub.TypesSubsidy.Code != "MATERNITY")
                    .FirstOrDefaultAsync(cancellationToken);

                if (subsidy is not null)
                {
                    daysWithSubsidy = subsidy.AmountDays;
                }

                decimal valueVacationsDay = valueVacations / 15;
                decimal amountToDiscountBySubsidy = daysWithSubsidy * valueVacationsDay;

                var vacationControl = await _unitOfWork.Vacations.Entities
                    .Where(col => col.CollaboratorId == collaborator.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (vacationControl is null)
                {
                    _logger.LogInformation("No se encontro el registro y control de vacaciones");
                    continue;
                }

                vacationControl.AvailableVacations += valueVacations - amountToDiscountBySubsidy;
                vacationControl.GeneredVacation += valueVacations - amountToDiscountBySubsidy;

                await _unitOfWork.Vacations.UpdateAsync(vacationControl);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}