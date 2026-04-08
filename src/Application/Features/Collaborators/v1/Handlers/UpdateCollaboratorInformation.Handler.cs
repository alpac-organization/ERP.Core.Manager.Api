using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;
using ERP.Core.Application.Commons.Interfaces;

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
                if (request.WorkingInformation != null)
                {
                    // Forzamos el valor actual si el del request es nulo
                    // WorkingInformation?.BranchId = request.WorkingInformation?.BranchId ?? WorkingInformation?.BranchId;
                    // WorkingInformation?.WorkAreaId = request.WorkingInformation?.WorkAreaId ?? WorkingInformation?.WorkAreaId;
                    WorkingInformation?.InssNumber = request?.WorkingInformation?.InssNumber ?? WorkingInformation?.InssNumber;
                    WorkingInformation?.BankAccountNumber = request?.WorkingInformation?.BankAccountNumber ?? WorkingInformation?.BankAccountNumber;
                }
            }
            if(access.Role.RoleType != RoleType.Supervisor)
            {
                if (request?.PersonalInformation is not null)
                {
                    PersonalInformation?.PersonalEmail = request.PersonalInformation?.PersonalEmail ?? PersonalInformation?.PersonalEmail;
                    PersonalInformation?.MaritalStatus = request.PersonalInformation?.MaritalStatus ?? PersonalInformation.MaritalStatus;
                    PersonalInformation?.PersonalPhoneNumber = request.PersonalInformation?.PersonalPhoneNumber ?? PersonalInformation.PersonalPhoneNumber;
                    PersonalInformation?.Address = request.PersonalInformation?.Address ?? PersonalInformation.Address;
                }

                if(request?.WorkingInformation is not null)
                {
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