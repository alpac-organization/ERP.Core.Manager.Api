using MediatR;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.SalaryAdvance.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.SalaryAdvance.v1.Handlers
{
    public class RegisterSalaryAdvanceHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager): AlpacBaseHandler<RegisterSalaryAdvanceCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(RegisterSalaryAdvanceCommand request, CancellationToken cancellationToken)
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

            var payrolActive = await _unitOfWork.Payrolls.Entities
                .Where(payroll => payroll.Status == PayrollStatus.Progress && payroll.PayrollType == PayrollType.Ordinary)
                .Where(payroll => payroll.BranchId == collaborator.WorkingInformation.CompanyBranchId)
                .FirstOrDefaultAsync(cancellationToken);

            var AdvanceSalary =  await _unitOfWork.Deductions.Entities
                .Where(deduction => deduction.CollaboratorId == collaborator.Id)
                .Where(deduction => deduction.Type == DeductionType.SalaryAdvance && deduction.Status == DeductionStatus.Progress)
                .FirstOrDefaultAsync(cancellationToken);

            if (AdvanceSalary is not null)
            {
                return _errorManager.ThrowBadRequest<bool>($"Este colaborador ya ha solicitado un adelanto de C${AdvanceSalary.AmountPaid}", "ERP:02");
            }

            var salaryInformation = await _unitOfWork.Salaries.Entities
                .Where(sal => sal.CollaboratorId == collaborator.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (salaryInformation is null)
            {
                return _errorManager.ThrowBadRequest<bool>("No se encontro la información salarial de este colaborador", "ERP:01");
            }

            if (salaryInformation.SalaryType != SalaryType.Fixed)
            {
                return _errorManager.ThrowBadRequest<bool>("Solo colaboradores de salario fijo pueden adelantar salario", "ERP:02");
            }

            decimal RequestedQuantity = request?.Amount ?? 0.0m;                    

            if (request?.Currency == Currency.USD)
            {
                RequestedQuantity = request.Amount * 36.6243m;
            }

            decimal BiweeklySalary = salaryInformation.AmountInLocal / 2;

            if(RequestedQuantity > BiweeklySalary)
            {
                return _errorManager.ThrowBadRequest<bool>($"Solo puedes adelantar: C${BiweeklySalary}", "ERP:02");
            }

            if (payrolActive is not null)
            {
                var payrollInformation = await _unitOfWork.OrdinaryPayrolls.Entities
                    .Include(payroll => payroll.Payroll)
                    .FirstOrDefaultAsync(pay => 
                        pay.CollaboratorId == collaborator.Id && 
                        pay.Payroll.Id == payrolActive.Id,
                        cancellationToken);

                if (payrollInformation is null)
                {
                    return _errorManager.ThrowBadRequest<bool>(
                        "El colaborador no tiene un registro de pago generado en la nómina actual.", "ERP:03");
                }

                // Deserialización y actualización de montos
                var deductionsData = JsonSerializer.Deserialize<DeductionsAdditionalData>(
                    payrollInformation.DeductionsAdditionalData ?? "{}");

                if (deductionsData != null)
                {
                    deductionsData.SalaryAdvance = RequestedQuantity;
                    
                    payrollInformation.DeductionsAdditionalData = JsonSerializer.Serialize(deductionsData);
                    
                    payrollInformation.TotalToPay -= RequestedQuantity;
                    payrollInformation.TotalDeducctions += RequestedQuantity;

                    await _unitOfWork.OrdinaryPayrolls.UpdateAsync(payrollInformation);

                    var deduction = new Deduction()
                    {
                        TotalAmount = RequestedQuantity,
                        TotalAmountInDollars = RequestedQuantity / 36.6243m,
                        CollaboratorId = collaborator.Id,
                        PayrollId = payrolActive.Id,
                        Currency = request?.Currency ?? Currency.NIO,
                        Status = DeductionStatus.Progress,
                        Description = request?.Description ?? "Sin descripción",
                        Type = DeductionType.SalaryAdvance,
                    };

                    //Registrar Deducción
                    await  _unitOfWork.Deductions.RegisterDeduction(deduction);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

            }

            return true;
        }
    }
}