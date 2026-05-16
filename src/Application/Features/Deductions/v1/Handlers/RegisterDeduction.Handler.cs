using MediatR;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Handlers
{
    public class RegisterDeductionHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<RegisterDeductionHandler> _logger): AlpacBaseHandler<RegisterDeductionCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(RegisterDeductionCommand request, CancellationToken cancellationToken)
        {
            
            #pragma warning disable CA1873

            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            if (access.Role!.RoleType != RoleType.Administrator)
            {
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para registrar una dedución", "ERP:01");
            }

            var payrollActive = await _unitOfWork.Payrolls.Entities
                .Where(payroll => payroll.Status == PayrollStatus.Progress && payroll.PayrollType == PayrollType.Ordinary)
                .Where(payroll => payroll.Id == request.PayrollId)
                .FirstOrDefaultAsync(cancellationToken);

            if(payrollActive is null)
            {
                return _errorManager.ThrowBadRequest<bool>("El periodo seleccionado de nomina, no existe", "ERP:PeriodNotExist");
            }    

            switch(request.DeductionType)
            {
                case DeductionType.SalaryAdvance:
                {                    
                    return _errorManager.ThrowBadRequest<bool>("El servidor se encuentra en proceso de mejorar para traerte mas funcionalidades", "ERP:01");
                    // var payload = new RegisterSalaryAdvanceCommand
                    // {
                    //     UserId = request.UserId,
                    //     Amount = request.AdvanceSalaryPayload?.Amount ?? 0.0m,
                    //     CollaboratorId = request.AdvanceSalaryPayload?.CollaboratorId ?? Guid.Parse(string.Empty),
                    //     Currency = request.AdvanceSalaryPayload?.Currency ?? Currency.NIO,
                    //     ModuleCode = request.ModuleCode,
                    //     CompanyId = request.CompanyId
                    // };

                    // await _mediator.Send(payload, cancellationToken);

                    // return true;
                }
                case DeductionType.LateArrivals:
                {
                    //Datos importados desde el archivo.
                    foreach (var collaborator in request.LateArrivalsData)
                    {
                        var collaboratorInformation = await _unitOfWork.Collaborators.Entities
                            .Where(col => col.IdentificationNumber == collaborator.IdentificationNumber && col.CompanyId == request.CompanyId && col.Status != CollaboratorStatus.Inactive)
                            .Include(col => col.WorkingInformation)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (collaboratorInformation is null)
                        {
                            _logger.LogInformation("No se encontro al colaborador con cedula: {identificacion}", collaborator.IdentificationNumber);
                            continue;   
                        }

                        //Iniciamos el proceso de deducciones de llegadas tardes.
                        _logger.LogInformation("🚩Iniciando registro de llegadas tardes, collaborador: {identificacion}", collaborator.IdentificationNumber);
                        
                        var salaryInformation = await _unitOfWork.Salaries.Entities
                            .Where(col => col.CollaboratorId == collaboratorInformation.Id)
                            .Where(col => col.EndDate == null && col.SalaryType == SalaryType.Fixed)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (salaryInformation is null)
                        {
                            return _errorManager.ThrowBadRequest<bool>("No se pudo obtener la información salarial", "ERP:03");
                        }

                        //Calculo de valor por horas extras.
                        decimal DailySalary   = salaryInformation.AmountInLocal / 30;
                        decimal HourlyWage    = DailySalary / 8;
                        decimal PerMinuteWage = HourlyWage / 60;
                    
                        decimal TotalDeductionToLateArrivals = collaborator.TotalMinutes * PerMinuteWage;

                        _logger.LogInformation("Actualizando nomina para colaborador con cedula: {}", collaborator.IdentificationNumber);

                        var ordinaryPayroll = await _unitOfWork.OrdinaryPayrolls.Entities
                            .Where(col => col.CollaboratorId == collaboratorInformation.Id)
                            .Where(col => col.PayrollId == payrollActive.Id)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (ordinaryPayroll is null)
                        {
                            _logger.LogInformation("No se encontro registro de nomina de este colaborador => {identificacion}", collaborator.IdentificationNumber);
                            continue;
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

                        decimal total = ordinaryPayroll.TotalIncome - ordinaryPayroll.TotalLegalDeductions - totalDeductions + ordinaryPayroll.TotalTravelExpenses;

                        ordinaryPayroll.TotalToPay = total;
                        ordinaryPayroll.TotalDeducctions = ordinaryPayroll.TotalLegalDeductions + totalDeductions;

                        ordinaryPayroll.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);

                        await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayroll);
                        _logger.LogInformation("Se finaliza el proceso de actualización de datos de nomina");

                        await _unitOfWork.Deductions.RegisterDeduction(new()
                        {
                            Currency             = Currency.NIO,
                            Status               = DeductionStatus.Completed,
                            Type                 = DeductionType.LateArrivals,
                            CollaboratorId       = collaboratorInformation.Id,
                            Description          = "Llegadas tardes",
                            TotalAmount          = TotalDeductionToLateArrivals,
                            TotalAmountInDollars = TotalDeductionToLateArrivals / 36.6242m,
                        });

                        _logger.LogInformation("Se registro el proceso de deducción de llagadas tardes");
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("✅ registro actualizado con exito!");

                    return true;    
                }
                case DeductionType.Purisima:
                {
                    return _errorManager.ThrowBadRequest<bool>("El servidor se encuentra en proceso de mejorar para traerte mas funcionalidades", "ERP:01");
                    
                    // var ordinaryPayroll = await _unitOfWork.OrdinaryPayrolls.Entities
                    //     .Where(ord => ord.CollaboratorId == collaborator.Id)
                    //     .FirstOrDefaultAsync(cancellationToken);

                    // if (ordinaryPayroll is null)
                    // {
                    //     return false;
                    // }

                    //  var deductions =
                    //     JsonSerializer.Deserialize<DeductionsAdditionalData>(
                    //         ordinaryPayroll.DeductionsAdditionalData
                    //     ) ?? new DeductionsAdditionalData();


                    // deductions.Purisima = request.PurisimaPayload?.Amount ?? 0.0m;

                    // decimal totalDeductions =
                    //     deductions.Loans
                    //     + deductions.Purisima
                    //     + deductions.ChildSupportGarnishment
                    //     + deductions.SalaryAdvance
                    //     + deductions.ChristmasBonusAdvance
                    //     + deductions.JudicialSeizures
                    //     + deductions.UniformDeduction
                    //     + deductions.CashShortage
                    //     + deductions.OtherDeductions
                    //     + deductions.DeductionForLossesBulk
                    //     + deductions.Absences
                    //     + deductions.Sanction
                    //     + deductions.LateArrivals;

                    // decimal total = ordinaryPayroll.GrossSalary - ordinaryPayroll.TotalLegalDeductions - totalDeductions + ordinaryPayroll.TotalTravelExpenses;

                    // ordinaryPayroll.TotalToPay = total;
                    // ordinaryPayroll.DeductionsAdditionalData = JsonSerializer.Serialize(deductions);

                    // await _unitOfWork.OrdinaryPayrolls.UpdateAsync(ordinaryPayroll);

                    // await _unitOfWork.Deductions.RegisterDeduction(new()
                    // {
                    //     Type = DeductionType.Purisima,
                    //     CollaboratorId = request.CollaboratorId,
                    //     Currency = Currency.NIO,
                    //     Status = DeductionStatus.Progress,
                    //     PayrollId = payrollActive.Id,
                    //     TotalAmount = request.PurisimaPayload?.Amount ?? 0.0m,
                    //     TotalAmountInDollars = (request.PurisimaPayload?.Amount ?? 0.0m) / 36.6243m,
                    // });

                    // await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                case DeductionType.Loans:
                {
                    return _errorManager.ThrowBadRequest<bool>("El servidor se encuentra en proceso de mejorar para traerte mas funcionalidades", "ERP:01");
                    //Prestamos si o si es uno a uno
                }
                default:
                {
                    return _errorManager.ThrowBadRequest<bool>("Este tipo de deduccion no se encuentra disponible", "ERP:01");  
                }
            }

            #pragma warning restore CA1873
        }
    }
}