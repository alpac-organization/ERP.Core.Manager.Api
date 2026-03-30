using AutoMapper;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;

using Entities = ERP.Core.Manager.Api.Domain.Entities.Payroll;
using Commands = ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{

    public class CollaboratorProfile : Profile
    {
        public CollaboratorProfile()
        {
            CreateMap<Collaborator, GetCollaboratorDto>()
                .ForMember(dest => dest.CollaboratorId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.FirstLastname, opt => opt.MapFrom(src => src.FirstLastname))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => 
                string.Join(" ", new[] 
                { 
                    src.FirstName.ToCapitalize(), 
                    src.SecondName.ToCapitalize(), 
                    src.FirstLastname.ToCapitalize(), 
                    src.SecondLastname.ToCapitalize() 
                }.Where(s => !string.IsNullOrWhiteSpace(s)))))

                .ForMember(dest => dest.WorkArea, opt => opt.MapFrom(src => 
                    src.WorkingInformation != null && src.WorkingInformation.WorkArea != null 
                    ? src.WorkingInformation.WorkArea.CatalogName 
                    : string.Empty))
                
                .ForMember(dest => dest.WorkPosition, opt => opt.MapFrom(src => 
                    src.WorkingInformation != null && src.WorkingInformation.WorkPosition != null 
                    ? src.WorkingInformation.WorkPosition.CatalogName 
                    : string.Empty))
                    
                .ForMember(dest => dest.CollaboratorCode, opt => opt.MapFrom(src => src.CollaboratorCode))
                .ForMember(dest => dest.IdentificationNumber, opt => opt.MapFrom(src => src.IdentificationNumber));
        }
    }

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
                WorkPhoneNumber = info.WorkPhoneNumber,
                WorkEmail = info.WorkEmail,
                InssNumber = info.InssNumber,
                EntryDate = DateTime.UtcNow 
            };
        }
    }
}