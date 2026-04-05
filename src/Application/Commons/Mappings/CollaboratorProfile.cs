using AutoMapper;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;

using Commands = ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{

    public class CollaboratorProfile : Profile
    {
        public CollaboratorProfile()
        {

            #region Mapeo de listado de colaboradores

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
                
            #endregion

            #region Mapeo de detalles de colaborador

            CreateMap<PersonalInformation, PersonalInformationDto>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.PersonalEmail, opt => opt.MapFrom(src => src.PersonalEmail))
                .ForMember(dest => dest.Birthdate, opt => opt.MapFrom(src => src.Birthdate))
                .ForMember(dest => dest.PersonalPhoneNumber, opt => opt.MapFrom(src => src.PersonalPhoneNumber));

            CreateMap<WorkingInformation, WorkingInformationDto>()
                .ForMember(dest => dest.WorkArea, opt => opt.MapFrom(src => src.WorkArea.CatalogName))
                .ForMember(dest => dest.WorkEmail, opt => opt.MapFrom(src => src.WorkEmail))
                .ForMember(dest => dest.WorkPhoneNumber, opt => opt.MapFrom(src => src.WorkPhoneNumber))
                .ForMember(dest => dest.EntryDate, opt => opt.MapFrom(src => src.EntryDate))
                .ForMember(dest => dest.WorkPosition, opt => opt.MapFrom(src => src.WorkPosition.CatalogName))
                .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.Branch.CatalogName));

            CreateMap<Salary, SalaryInformationDto>()
                .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.AmountSalary))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency.ToString()))
                .ForMember(dest => dest.SalaryType, opt => opt.MapFrom(src => src.SalaryType.ToString()));

            CreateMap<Collaborator, CollaboratorDetailsDto>()
                .ForMember(dest => dest.CollaboratorId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.WorkPosition, opt => opt.MapFrom(src => src.WorkingInformation.WorkPosition.CatalogName))
                .ForMember(dest => dest.CollaboratorCode, opt => opt.MapFrom(src => src.CollaboratorCode))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => 
                    string.Join(" ", new[] 
                    { 
                        src.FirstName, src.SecondName, src.FirstLastname, src.SecondLastname 
                    }.Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.ToCapitalize()))))
                
                .ForMember(dest => dest.PersonalInformation, opt => opt.MapFrom(src => src.PersonalInformation))
                .ForMember(dest => dest.WorkingInformation, opt => opt.MapFrom(src => src.WorkingInformation))
                .ForMember(dest => dest.SalaryInformation, opt => opt.MapFrom(src => src.Salaries != null ? src.Salaries.FirstOrDefault() : null))

                
                .AfterMap((src, dest) => {
                    if (dest.PersonalInformation != null)
                    {
                        dest.PersonalInformation.Gender = src.Gender;
                        dest.PersonalInformation.IdentificationNumber = src.IdentificationNumber;
                        dest.PersonalInformation.PersonalEmail = src.PersonalInformation.PersonalEmail;
                    }
                });

            #endregion   
        }
    }

    #region Mapeo para crear colaborador
    public static class CollaboratorMapper
    {
        public static Collaborator ToCollaboratorEntity(this Commands.RegisterCollaboratorCommand command, string generatedCode)
        {
            return new Collaborator
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
                RegisteredBy = command.RegisteredBy ?? "Sistema ERP",
                PictureUrl = null
            };
        }

        public static PersonalInformation ToPersonalInformationEntity(this Commands.PersonalInformation info, Guid collaboratorId)
        {
            return new PersonalInformation
            {
                Id = Guid.NewGuid(),
                CollaboratorId = collaboratorId,
                Address = info.Address,
                PersonalEmail = info.PersonalEmail,
                PersonalPhoneNumber = info.PersonalPhoneNumber,
                Departament = info.Departament,
                Birthdate = info.Birthdate,
                MaritalStatus = info.MaritalStatus
            };
        }

        public static WorkingInformation ToWorkingInformationEntity(this Commands.WorkingInformation info, Guid collaboratorId)
        {
            return new WorkingInformation
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

        #endregion Mapeo para crear colaborador
    }
}