using Entities = ERP.Core.Manager.Api.Domain.Entities.Payroll;
using Commands = ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public static class CollaboratorMapper
    {
        public static Entities.Collaborator ToCollaboratorEntity(this Commands.RegisterCollaboratorCommand command, string generatedCode)
        {
            return new Entities.Collaborator
            {
                Id = Guid.NewGuid(),
                CompanyId = command.CompanyId,
                FirstName = command.FirstName,
                SecondName = command.SecondName,
                ThirdName = command.ThirdName,
                FirstLastname = command.FirstLastname,
                SecondLastname = command.SecondLastname,
                IdentificationNumber = command.IdentificationNumber,
                IdentificationType = command.IdentificationType,
                Gender = command.Gender,
                Status = command.Status,
                CollaboratorCode = generatedCode,
                RegisteredBy = command.RegisteredBy ?? "Sistema ERP"
            };
        }

        public static Entities.PersonalInformation ToPersonalInformationEntity(this Commands.PersonalInformation info, Guid collaboratorId)
        {
            return new Entities.PersonalInformation
            {
                Id = Guid.NewGuid(),
                CollaboratorId = collaboratorId,
                Address = info.Address,
                PersonalEmail = info.PersonalEmail,
                PersonalPhoneNumber = info.PersonalPhoneNumber,
                Departament = info.Departament,
                Birthdate = info.Birthdate
            };
        }

        public static Entities.WorkingInformation ToWorkingInformationEntity(this Commands.WorkingInformation info, Guid collaboratorId)
        {
            return new Entities.WorkingInformation
            {
                Id = Guid.NewGuid(),
                CollaboratorId = collaboratorId,
                WorkAreaId = info.WorkAreaId,
                WorkPositionId = info.WorkPositionId,
                BranchId = info.BranchId,
                BankAccountNumber = info.BankAccountNumber,
                WorkPhonNumber = info.WorkPhonNumber,
                WorkEmail = info.WorkEmail,
                InssNumber = info.InssNumber,
                EntryDate = DateTime.UtcNow 
            };
        }
    }
}