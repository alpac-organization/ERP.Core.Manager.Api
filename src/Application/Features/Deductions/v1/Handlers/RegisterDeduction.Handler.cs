using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Commands;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Handlers
{
    public class RegisterDeductionHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IDeductionsServices _deductionServices, ILogger<RegisterDeductionHandler> _logger): AlpacBaseHandler<RegisterDeductionCommand, bool>(_unitOfWork, _errorManager)
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
                    _logger.LogInformation("🚩Iniciando proceso de deducción de llegadas tardes");

                    switch (request.LateArrivalsInformation.ProcedureMethod)
                    {
                        case ProcedureMethod.Manual:
                        {
                            _logger.LogInformation("El proceso de deducción por llegadas tardes se realizara de forma manual");

                            var payload = request.LateArrivalsInformation.LateArrivalsPayload;

                            var collaboratorInformation = await _unitOfWork.Collaborators.Entities
                                .Where(col => col.IdentificationNumber == payload.IdentificationNumber && col.CompanyId == request.CompanyId && col.Status != CollaboratorStatus.Inactive)
                                .Include(col => col.WorkingInformation)
                                .FirstOrDefaultAsync(cancellationToken);

                            if (collaboratorInformation is null)
                            {
                                return _errorManager.ThrowBadRequest<bool>($"No se encontro al colaborador con cedula: {payload.IdentificationNumber}", "ERP:01");   
                            }

                            _logger.LogInformation("🚩Iniciando registro de llegadas tardes, collaborador: {identificacion}", payload.IdentificationNumber);
                                
                            var salaryInformation = await _unitOfWork.Salaries.Entities
                                .Where(col => col.CollaboratorId == collaboratorInformation.Id)
                                .Where(col => col.EndDate == null && col.SalaryType == SalaryType.Fixed)
                                .FirstOrDefaultAsync(cancellationToken);

                            if (salaryInformation is null)
                            {
                                return _errorManager.ThrowBadRequest<bool>($"No se pudo obtener la información salarial del colaborador con cedula: {payload.IdentificationNumber}", "ERP:01");
                            }

                            await _deductionServices.ApplyDeductionLateArrivals(collaboratorInformation, salaryInformation, payload.TotalMinutes, request.PayrollId);
                            
                            break;
                        }
                        case ProcedureMethod.Import:
                        {
                            _logger.LogInformation("El proceso de deducción por llegadas tardes se realizara mediante importación de datos");

                            foreach (var collaborator in request.LateArrivalsInformation.LateArrivalsData)
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
                            break;
                        }
                        default:
                        {
                            _logger.LogInformation("No se ha seleccionado un metodo valido para el registro de llegadas tardes");
                            return _errorManager.ThrowBadRequest<bool>("No se ha seleccionado un metodo valido para el registro de llegadas tardes", "ERP:01");
                        }
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("✅Se finaliza el proceso de deducción por horas extras");

                    return true;    
                }
                case DeductionType.Purisima:
                {                    
                    _logger.LogInformation("🚩Iniciando proceso de deducción por el dia de la purisima");

                    switch (request.PurisimaInformation.ProcedureMethod)
                    {
                        case ProcedureMethod.Manual:
                        {
                            _logger.LogInformation("El proceso de deducción por purisima se realizara de forma manual");

                            var payload = request.PurisimaInformation.PurisimaPayload;

                            var collaboratorInformation = await _unitOfWork.Collaborators.Entities
                                .Where(col => col.IdentificationNumber == payload.IdentificationNumber && col.CompanyId == request.CompanyId && col.Status != CollaboratorStatus.Inactive)
                                .Include(col => col.WorkingInformation)
                                .FirstOrDefaultAsync(cancellationToken);

                            if (collaboratorInformation is null)
                            {
                                return _errorManager.ThrowBadRequest<bool>($"No se encontro al colaborador con cedula: {payload.IdentificationNumber}", "ERP:01");   
                            }

                            var deductionActive = await _unitOfWork.Deductions.Entities
                                .Where(ded => ded.Type == request.DeductionType)
                                .Where(ded => ded.CollaboratorId == collaboratorInformation.Id)
                                .Where(ded => ded.Status == DeductionStatus.Progress)
                                .FirstOrDefaultAsync(cancellationToken);

                            if (deductionActive is not null)
                            {
                                return _errorManager.ThrowBadRequest<bool>("Este colaborador ya cuenta con una aportación de purisima","ERP");
                            }

                            _logger.LogInformation("🚩Iniciando registro de deducción por purisima, collaborador: {identificacion}", payload.IdentificationNumber);
                                
                            await _deductionServices.ApplyDeductionPurisima(collaboratorInformation, payload.Amount, request.PayrollId, payload.NumberFortnights);
                            
                            break;
                        }
                        case ProcedureMethod.Import:
                        {
                            var payload = request.PurisimaInformation;

                            foreach (var collaborator in payload.PurisimaData)
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

                                var deductionActive = await _unitOfWork.Deductions.Entities
                                    .Where(ded => ded.Type == request.DeductionType)
                                    .Where(ded => ded.CollaboratorId == collaboratorInformation.Id)
                                    .Where(ded => ded.Status == DeductionStatus.Progress)
                                    .FirstOrDefaultAsync(cancellationToken);

                                if (deductionActive is not null)
                                {
                                    _logger.LogInformation("El colaborador con cedula {identification} ya cuenta con una aporte de purisima activa", collaborator.IdentificationNumber);
                                    continue;
                                }

                                await _deductionServices.ApplyDeductionPurisima(collaboratorInformation, collaborator.Amount, payrollActive.Id, collaborator.NumberFortnights);
                            }

                            break;   
                        }
                        default:
                        {
                            _logger.LogInformation("No se ha seleccionado un metodo valido para el registro para la aportación de la purisima");
                            return _errorManager.ThrowBadRequest<bool>("No se ha seleccionado un metodo valido para el registro para la aportación de la purisima", "ERP:01");
                        }
                    }

                    //✅Finalizar Proceso y registrar deducción de forma activa como si fuera un prestamo.
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    
                    _logger.LogInformation("✅Se finaliza el proceso de deducción para la purisima🎆");

                    return true;
                }
                case DeductionType.Loans:
                {

                    _logger.LogInformation("🚩Iniciando proceso de deducción por prestamos");

                    var payload = request.LoansPayload ?? new ();

                    var collaboratorInformation = await _unitOfWork.Collaborators.Entities
                        .Where(col => col.IdentificationNumber == payload.IdentificationNumber && col.CompanyId == request.CompanyId && col.Status != CollaboratorStatus.Inactive)
                        .Include(col => col.WorkingInformation)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (collaboratorInformation is null)
                    {
                        return _errorManager.ThrowBadRequest<bool>($"No se encontro al colaborador con cedula: {payload.IdentificationNumber}", "ERP:01");   
                    }

                    _logger.LogInformation("🚩Iniciando registro de deducción por prestamos, collaborador: {identificacion}, data: {@data}", payload.IdentificationNumber, payload);

                    await _deductionServices.ApplyDeductionLoans(collaboratorInformation, payload.Amount, request.PayrollId, payload.NumberFortnights, payload.Currency, payload?.Description ?? "Registro de préstamo");
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    
                    _logger.LogInformation("✅Se finaliza el proceso de deducción por prestamos");
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
