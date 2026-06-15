using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Mappings;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;
using Microsoft.Extensions.Logging;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Handlers
{
    public class RegisterCollaboratorHandler(IUnitOfWork _unitOfWork, ILogger<RegisterCollaboratorHandler> _logger, IErrorManager _errorManager, ICodeGenerator _codeGenerator, IPayrollServices _payrollServices) : AlpacBaseHandler<RegisterCollaboratorCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(RegisterCollaboratorCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse; 
            }

            var existsInCompany = await _unitOfWork.Collaborators.Entities
                .AnyAsync(c => c.IdentificationNumber == request.IdentificationNumber && c.CompanyId == request.CompanyId, cancellationToken);

            if (existsInCompany)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    $"El número de identificación {request.IdentificationNumber} ya está registrado en esta empresa.", 
                    "ERP:001"
                );
            }

            bool isSuccess = true;

            var user = await _unitOfWork.Users.FirstOrDefaultAsync(user => user.Id == request.UserId, cancellationToken);

            if (access.Role!.RoleType == RoleType.Administrator || access.Role!.RoleType == RoleType.Operator)
            {
                #region Mapeo de campos.
                var code = _codeGenerator.GenerateModuleCode(request.IdentificationNumber!);
                request.RegisteredBy = user!.UserName;

                var collaboratorEntity = CollaboratorMapper.ToCollaboratorEntity(request, code);
                await _unitOfWork.Collaborators.RegisterCollaborator(collaboratorEntity);

                if (request.PersonalInformation != null)
                {
                    //Registramos su información personal
                    var personalInfo = CollaboratorMapper.ToPersonalInformationEntity(request.PersonalInformation, collaboratorEntity.Id);
                    personalInfo.CollaboratorId = collaboratorEntity.Id;

                    await _unitOfWork.PersonalInformations.RegisterPersonalInformation(personalInfo);
                }

                if (request.WorkingInformation != null)
                {
                    // Registramos su información laboral
                    var workingInfo = CollaboratorMapper.ToWorkingInformationEntity(request.WorkingInformation, collaboratorEntity.Id);
                    workingInfo.CollaboratorId = collaboratorEntity.Id;

                    await _unitOfWork.WorkingInformations.RegisterWorkingInformation(workingInfo);
                }
                #endregion

                #region Registrar salario laboral

                isSuccess = await _payrollServices.AssignSalary(collaboratorEntity, request.SalaryInformation ?? new());

                if (isSuccess is false)
                {
                    return _errorManager.ThrowBadRequest<bool>("No se pudo realizar la asignación de salario. consultar con IT", "ERP");
                }

                #endregion

                #region Asignación de vacaciones

                if (request.SalaryInformation!.SalaryType != SalaryType.ProfessionalServices)
                {
                    await _payrollServices.AssignVacationControl(collaboratorEntity);
                }

                #endregion

                #region Asignación de viaticos del colaborador.
                
                if (request?.TravelExpenses?.Count > 0)
                {
                    await _payrollServices.AssignTravelAllowance(collaboratorEntity, request.TravelExpenses);
                }

                #endregion

                //✅Colaborador registrado con exito
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                //Procesos para realizar insert a la nomina actual
                #region Insertar colaborador a la nomina si se encuentra activa

                _logger.LogInformation("Verificar si existe una nomina en progreso✅");

                var payroll = await _unitOfWork.Payrolls.Entities
                    .Where(pay => pay.Status == PayrollStatus.Progress)
                    .Where(pay => pay.BranchId == request!.WorkingInformation!.BranchId)
                    .FirstOrDefaultAsync(cancellationToken);

                if(payroll is not null)
                {
                    _logger.LogInformation("✅Nomina en progreso encontrada");

                    if (request!.SalaryInformation.SalaryType == SalaryType.Fixed)
                    {
                        var collaborator = await _unitOfWork.Collaborators.Entities
                            .Where(col => col.IdentificationNumber == request.IdentificationNumber)
                            .Include(col => col.WorkingInformation)
                                .ThenInclude(col => col.BranchInfo)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (collaborator is null)
                        {
                            _logger.LogInformation("No se encontro registros del colaborador recien ingresado");
                        }
                        else
                        {
                            await _payrollServices.RegisterCollaboratorToPayroll(payroll.Id, collaborator, cancellationToken);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                            
                            _logger.LogInformation("Colaborador ingresado correctamente a la nomina✅");
                        }
                    }    
                }

                #endregion

                return true;
            }
            else
            {
                return _errorManager.ThrowBadRequest<bool>("Este usuario no tiene permisos para crear este registro", "ERP:002");
            }
        }
    }
}   