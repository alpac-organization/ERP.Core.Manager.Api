using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Manager.Api.Application.Commons.Mappings;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;
using ERP.Core.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Handlers
{
    public class RegisterCollaboratorHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ICodeGenerator _codeGenerator)
    : AlpacBaseHandler<RegisterCollaboratorCommand, bool>(_unitOfWork, _errorManager)
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
                var code = _codeGenerator.GenerateModuleCode(request.IdentificationNumber!);
                request.RegisteredBy = user!.UserName;

                var collaboratorEntity = CollaboratorMapper.ToCollaboratorEntity(request, code);
                await _unitOfWork.Collaborators.RegisterCollaborator(collaboratorEntity,cancellationToken);

                if (request.PersonalInformation != null)
                {
                    //Registramos su información personal
                    var personalInfo = CollaboratorMapper.ToPersonalInformationEntity(request.PersonalInformation, collaboratorEntity.Id);
                    await _unitOfWork.PersonalInformations.RegisterPersonalInformation(personalInfo, cancellationToken);
                }

                if (request.WorkingInformation != null)
                {
                    // Registramos su información laboral
                    var workingInfo = CollaboratorMapper.ToWorkingInformationEntity(request.WorkingInformation, collaboratorEntity.Id);
                    await _unitOfWork.WorkingInformations.RegisterWorkingInformation(workingInfo, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Registrar Información Salarial
                var amountSalary = request.SalaryInformation?.Salary ?? 0;
                decimal amountInLocal = 0;
                decimal amountInForeign = 0;
                const decimal exchangeRate = 36.6243m;

                if (request.SalaryInformation!.Currency == Currency.USD)
                {
                    amountInLocal = amountSalary * exchangeRate;
                    amountInForeign = amountSalary;
                }
                else
                {
                    amountInLocal = amountSalary;
                    amountInForeign = amountSalary / exchangeRate;
                }

                var salary = new Salary()
                {
                    SalaryType = request.SalaryInformation.SalaryType,
                    BankSubCatalogId = request.SalaryInformation.SubCatalogBankId,
                    AmountSalary = amountSalary,
                    AmountInLocal = amountInLocal, 
                    AmountInForeign = amountInForeign,
                    Currency = request.SalaryInformation.Currency,
                    CollaboratorId = collaboratorEntity.Id,
                    StartDate = DateTime.Now
                };

                await _unitOfWork.Salaries.RegisterSalary(salary, cancellationToken);

                //Registramos su control de vacaciones. 
                var daysElapsed = CalculatorUtils.CalculateDaysElapsedCommercial(request?.WorkingInformation?.EntryDate ?? DateTime.Now);
                
                decimal generated = Math.Round((decimal)(daysElapsed * 30.0 / 360.0), 4);
                
                Vacation vacation = new ()
                {
                    CollaboratorId = collaboratorEntity.Id,
                    EnjoyedVacation = 0,
                    GeneredVacation = generated,
                    AvailableVacations = generated,
                };

                await _unitOfWork.Vacations.RegisterVacationControl(vacation, cancellationToken);
            }
            else
            {
                return _errorManager.ThrowBadRequest<bool>("Este usuario no tiene permisos para crear este registro", "ERP:002");
            }

            return true;
        }
    }
}   