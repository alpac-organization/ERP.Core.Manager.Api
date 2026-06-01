using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Handlers
{
    public class ClosePayrollProcessHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<ClosePayrollProcessHandler> _logger): AlpacBaseHandler<ClosePayrollProcessCommand, bool>(_unitOfWork, _errorManager)
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
                .FirstOrDefaultAsync(cancellationToken);

            if (payroll is null)
            {
                return  _errorManager.ThrowBadRequest<bool>("Esta nomina no se encuentra en curso o no existe", "ERP:02");
            }

            #endregion

            payroll.Status = PayrollStatus.Closed;
            await _unitOfWork.Payrolls.UpdateAsync(payroll);

            //Guardamos procesos de historiales
            var registers = payroll.OrdinaryPayrolls;

            //Realizar todo aquel, registro de deducciones y pagos realizados del colaborador
            foreach(var collaborator in registers)
            {
                var deductionsActive = await _unitOfWork.Deductions.Entities
                    .Where(deduction => deduction.CollaboratorId == collaborator.Id)
                    .Where(deduction => deduction.Status == DeductionStatus.Progress)
                    .ToListAsync(cancellationToken);

                foreach(var deduction in deductionsActive)
                {
                    var payment = await _unitOfWork.DeductionPaymentHistories.Entities
                        .Where(paid => paid.DeductionId == deduction.Id)
                        .Where(paid => paid.Status == DeductionPaymentStatus.Pending)
                        .Include(paid => paid.Deduction)
                        .FirstOrDefaultAsync(cancellationToken);

                    if(payment is null)
                    {
                        _logger.LogInformation("");
                        continue;
                    }
                    
                    decimal amountInLocal   = deduction.FortnightlyAmount ?? 0;
                    decimal amountInDollars = deduction.FortnightlyAmountInDollars ?? 0;

                    deduction.AmountPaid += amountInLocal;
                    deduction.AmountPaidInDollars += amountInDollars;
                    deduction.NumberFortnightsPaid += 1;

                    if (deduction.Type == DeductionType.Loans)
                    {
                        deduction.TotalBalance -= amountInLocal;
                        deduction.TotalBalanceInDollars -= amountInDollars;

                        if (deduction.TotalBalance <= 0 && deduction.TotalBalanceInDollars <= 0)
                        {
                            deduction.Status = DeductionStatus.Completed;
                            await _unitOfWork.Deductions.UpdateAsync(deduction);
                        }

                        payment.Status = DeductionPaymentStatus.Paid;
                        await _unitOfWork.DeductionPaymentHistories.UpdateAsync(payment);
                    }

                    if (deduction.Type == DeductionType.Purisima)
                    {
                        payment.Status = DeductionPaymentStatus.Paid;
                        await _unitOfWork.DeductionPaymentHistories.UpdateAsync(payment);
                    }

                    if (deduction.Type == DeductionType.OtherDeductions)
                    {
                        deduction.TotalBalance -= amountInLocal;
                        deduction.TotalBalanceInDollars -= amountInDollars;

                        if (deduction.TotalBalance <= 0 && deduction.TotalBalanceInDollars <= 0)
                        {
                            deduction.Status = DeductionStatus.Completed;
                            await _unitOfWork.Deductions.UpdateAsync(deduction);
                        }

                        payment.Status = DeductionPaymentStatus.Paid;
                        await _unitOfWork.DeductionPaymentHistories.UpdateAsync(payment);
                    }

                    await _unitOfWork.Deductions.UpdateAsync(deduction);
                }


                //Acumulado de vacaciones
                var vacationControl = await _unitOfWork.Vacations.Entities
                    .Where(col => col.CollaboratorId == collaborator.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (vacationControl is null)
                {
                    _logger.LogInformation("No se encontro el registro y control de vacaciones");
                    continue;
                }

                vacationControl.AvailableVacations += 1.25m;
                vacationControl.GeneredVacation+= 1.25m;
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}   