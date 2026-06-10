using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Handlers
{
    public class UpdateCollaboratorInformationHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager): AlpacBaseHandler<UpdateCollaboratorInformationCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(UpdateCollaboratorInformationCommand request, CancellationToken cancellationToken)
        {

            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse; 
            }

            var collaborator = await _unitOfWork.Collaborators.Entities
                .Where(col => col.IdentificationNumber == request.IdentificationNumber)
                .Include(col => col.PersonalInformation)
                .Include(col => col.WorkingInformation)
                .FirstOrDefaultAsync(cancellationToken);

            var PersonalInformation = collaborator?.PersonalInformation;
            var WorkingInformation = collaborator?.WorkingInformation;
    
            if (collaborator is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Este colaborador no existe en nuestro sistema", "ERP:001");
            }

            if (access.Role!.RoleType == RoleType.Administrator || access.Role.RoleType == RoleType.Manager)
            {
                collaborator.FirstName = request.FirstName ?? collaborator.FirstName;
                collaborator.SecondName = request.SecondName ?? collaborator.SecondName;
                collaborator.ThirdName = request.ThirdName ?? collaborator.ThirdName;
                collaborator.FirstLastname = request.FirstSurname ?? collaborator.FirstLastname;
                collaborator.SecondLastname = request.SecondSurname ?? collaborator.SecondLastname;
                collaborator.CollaboratorCode = request.CodeCollaborator ?? collaborator.CollaboratorCode;

                if (request.WorkingInformation != null)
                {
                    if (request.WorkingInformation.WorkAreaId.HasValue)
                    {
                        WorkingInformation?.WorkAreaId = request.WorkingInformation.WorkAreaId.Value;                    
                    }

                    if (request.WorkingInformation.WorkPositionId.HasValue)
                    {
                        WorkingInformation?.WorkPositionId = request.WorkingInformation.WorkPositionId.Value;                    
                    }
                    
                    WorkingInformation?.InssNumber = request?.WorkingInformation?.InssNumber ?? WorkingInformation?.InssNumber;
                    WorkingInformation?.BankAccountNumber = request?.WorkingInformation?.BankAccountNumber ?? WorkingInformation?.BankAccountNumber;
                }
            }
            if(access.Role.RoleType != RoleType.Supervisor)
            {
                if (request?.PersonalInformation is not null)
                {
                    PersonalInformation?.Address = request.PersonalInformation?.Address ?? PersonalInformation.Address;
                    PersonalInformation?.PersonalEmail = request.PersonalInformation?.PersonalEmail ?? PersonalInformation?.PersonalEmail;
                    PersonalInformation?.MaritalStatus = request.PersonalInformation?.MaritalStatus ?? PersonalInformation.MaritalStatus;
                    PersonalInformation?.PersonalPhoneNumber = request.PersonalInformation?.PersonalPhoneNumber ?? PersonalInformation.PersonalPhoneNumber;
                }

                if(request?.WorkingInformation is not null)
                {
                    WorkingInformation?.Daem = request.WorkingInformation.Daem ?? WorkingInformation.Daem;
                    WorkingInformation?.WorkPhoneNumber = request.WorkingInformation?.WorkPhoneNumber ?? WorkingInformation.WorkPhoneNumber;
                    WorkingInformation?.WorkEmail = request.WorkingInformation?.WorkEmail ?? WorkingInformation.WorkEmail;  
                }
            }

            await _unitOfWork.Collaborators.UpdateAsync(collaborator);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}