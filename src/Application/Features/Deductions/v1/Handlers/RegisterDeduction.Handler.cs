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
using ERP.Core.Manager.Api.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Handlers
{
    public class RegisterDeductionHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IDeductionsService _deductionServices, ILogger<RegisterDeductionHandler> _logger): AlpacBaseHandler<RegisterDeductionCommand, bool>(_unitOfWork, _errorManager)
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
                .Where(payroll => payroll.Status == PayrollStatus.Progress)
                .Where(payroll => payroll.Id == request.PayrollId)
                .FirstOrDefaultAsync(cancellationToken);

            if(payrollActive is null)
            {
                return _errorManager.ThrowBadRequest<bool>("El periodo seleccionado de nomina, no existe", "ERP:PeriodNotExist");
            }    

            switch(request.DeductionType)
            {
                case DeductionType.LateArrivals:
                {
                    //Datos importados desde el archivo.
                    _logger.LogInformation("🚩Iniciando proceso de deducción de llegadas tardes");

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
                            _logger.LogInformation("No se pudo obtener la información salarial del colaborador con cedula: {identificacion}", collaborator.IdentificationNumber);
                            continue;
                        }

                        await _deductionServices.ApplyDeductionLateArrivals(collaboratorInformation, salaryInformation, collaborator.TotalMinutes, request.PayrollId);
                    }

                    //Guardamos cambios en la base de datos.
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("✅Se finaliza el proceso de deducción por horas extras");
                    return true;    
                }
                case DeductionType.Purisima:
                {                    
                    _logger.LogInformation("🚩Iniciando proceso de deducción por el dia de la purisima");

                    foreach (var collaborator in request.PurisimaData)
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

                        await _deductionServices.ApplyDeductionPurisima(collaboratorInformation, collaborator.Amount, payrollActive.Id);
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    
                    _logger.LogInformation("✅Se finaliza el proceso de deducción pora la purisima🎆");

                    return true;
                }
                case DeductionType.Loans:
                {
                    return _errorManager.ThrowBadRequest<bool>("El servidor se encuentra en proceso de mejorar para traerte mas funcionalidades", "ERP:01");
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
