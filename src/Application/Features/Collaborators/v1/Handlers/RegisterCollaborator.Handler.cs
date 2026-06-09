using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Mappings;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Handlers
{
    public class RegisterCollaboratorHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ICodeGenerator _codeGenerator, IPayrollServices _payrollServices) : AlpacBaseHandler<RegisterCollaboratorCommand, bool>(_unitOfWork, _errorManager)
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

                await _payrollServices.AssignSalary(collaboratorEntity, request.SalaryInformation ?? new());

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

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return true;
            }
            else
            {
                return _errorManager.ThrowBadRequest<bool>("Este usuario no tiene permisos para crear este registro", "ERP:002");
            }
        }
    }
}   