using MediatR;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.SalaryAdvance.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Handlers
{
    public class RegisterDeductionHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMediator _mediator): AlpacBaseHandler<RegisterDeductionCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(RegisterDeductionCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            if (access.Role!.RoleType != RoleType.Administrator)
            {
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para registrar una dedución", "ERP:01");
            }

            var collaborator = await _unitOfWork.Collaborators.Entities
                .Where(col => col.Id == request.CollaboratorId && col.CompanyId == request.CompanyId && col.Status != CollaboratorStatus.Inactive)
                .Include(col => col.WorkingInformation)
                .FirstOrDefaultAsync(cancellationToken);

            if (collaborator is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Este colaborador no existe!", "ERP:01");
            }

            var payrollActive = await _unitOfWork.Payrolls.Entities
                .Where(payroll => payroll.Status == PayrollStatus.Progress && payroll.PayrollType == PayrollType.Ordinary)
                .Where(payroll => payroll.BranchId == collaborator.WorkingInformation.CompanyBranchId)
                .FirstOrDefaultAsync(cancellationToken);

            if (payrollActive is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Inicialize un proceso de nomina ordinaria", "ERP:02");
            }

            switch(request.DeductionType)
            {
                case DeductionType.SalaryAdvance:
                {                    
                    var payload = new RegisterSalaryAdvanceCommand
                    {
                        UserId = request.UserId,
                        Amount = request.AdvanceSalaryPayload?.Amount ?? 0.0m,
                        CollaboratorId = request.CollaboratorId,
                        Currency = request.AdvanceSalaryPayload?.Currency ?? Currency.NIO,
                        ModuleCode = request.ModuleCode,
                        CompanyId = request.CompanyId
                    };

                    await _mediator.Send(payload, cancellationToken);

                    return true;
                }
                case DeductionType.LateArrivals:
                {

                    var salaryInformation = await _unitOfWork.Salaries.Entities
                        .Where(col => col.CollaboratorId == collaborator.Id)
                        .Where(col => col.EndDate == null)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (salaryInformation is null)
                    {
                        return _errorManager.ThrowBadRequest<bool>("No se pudo obtener la información salarial", "ERP:03");
                    }

                    decimal DailySalary = salaryInformation.AmountInLocal / 30;
                    decimal HourlyWage = DailySalary / 8;
                    decimal PerMinuteWage = HourlyWage / 60;

                    decimal TotalDeductionToLateArrivals = (request.LateArrivalsPayload?.TotalMinutes ?? 0) * PerMinuteWage;

                    var ordinaryPayroll = await _unitOfWork.OrdinaryPayrolls.Entities
                        .Where(col => col.CollaboratorId == collaborator.Id)
                        .Where(col => col.PayrollId == payrollActive.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (ordinaryPayroll is null)
                    {
                        return false;
                    }

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

                    decimal total = ordinaryPayroll.GrossSalary - ordinaryPayroll.TotalLegalDeductions - totalDeductions + ordinaryPayroll.TotalTravelExpenses;

                    ordinaryPayroll.TotalToPay = total;
                    ordinaryPayroll.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);

                    await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayroll);
                    await _unitOfWork.Deductions.RegisterDeduction(new()
                    {
                        Type           = DeductionType.Purisima,
                        Currency       = Currency.NIO,
                        Status         = DeductionStatus.Completed,
                        PayrollId      = payrollActive.Id,
                        CollaboratorId = request.CollaboratorId,
                        TotalAmount    = TotalDeductionToLateArrivals,
                        TotalAmountInDollars = TotalDeductionToLateArrivals / 36.6243m,
                    });

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    return true;    
                }
                case DeductionType.Purisima:
                {

                    var ordinaryPayroll = await _unitOfWork.OrdinaryPayrolls.Entities
                        .Where(ord => ord.CollaboratorId == collaborator.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (ordinaryPayroll is null)
                    {
                        return false;
                    }

                     var deductions =
                        JsonSerializer.Deserialize<DeductionsAdditionalData>(
                            ordinaryPayroll.DeductionsAdditionalData
                        ) ?? new DeductionsAdditionalData();


                    deductions.Purisima = request.PurisimaPayload?.Amount ?? 0.0m;

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

                    await _unitOfWork.Deductions.RegisterDeduction(new()
                    {
                        Type = DeductionType.Purisima,
                        CollaboratorId = request.CollaboratorId,
                        Currency = Currency.NIO,
                        Status = DeductionStatus.Progress,
                        PayrollId = payrollActive.Id,
                        TotalAmount = request.PurisimaPayload?.Amount ?? 0.0m,
                        TotalAmountInDollars = (request.PurisimaPayload?.Amount ?? 0.0m) / 36.6243m,
                    });

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    return true;                
                }
                case DeductionType.Loans:
                {
                    


                    return true;
                }
                default:
                {
                    return _errorManager.ThrowBadRequest<bool>("Este tipo de deduccion no se encuentra disponible", "ERP:01");  
                }
            }
        }
    }
}