using AutoMapper;
using ERP.Core.Database.Domain.Entities.Payrolls;
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

             .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src =>
                 src.WorkingInformation != null && src.WorkingInformation.BranchInfo != null
                 ? src.WorkingInformation.BranchInfo.BranchName
                 : string.Empty))

             .ForMember(dest => dest.Vacations, opt => opt.MapFrom(src =>
                 src.WorkingInformation != null && src.Vacation != null
                 ? src.Vacation.AvailableVacations
                 : 0))

             .ForMember(dest => dest.CollaboratorCode, opt => opt.MapFrom(src => src.CollaboratorCode))

             .ForMember(dest => dest.IdentificationNumber, opt => opt.MapFrom(src => src.IdentificationNumber));

         #endregion

         #region Mapeo de detalles de colaborador

         CreateMap<PersonalInformation, PersonalInformationDto>()
             .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
             .ForMember(dest => dest.PersonalEmail, opt => opt.MapFrom(src => src.PersonalEmail))
             .ForMember(dest => dest.Birthdate, opt => opt.MapFrom(src => src.Birthdate))
             .ForMember(dest => dest.Departament, opt => opt.MapFrom(src => src.Departament != null ? src.Departament.CatalogName : null))
             .ForMember(dest => dest.PersonalPhoneNumber, opt => opt.MapFrom(src => src.PersonalPhoneNumber));

         CreateMap<WorkingInformation, WorkingInformationDto>()
             .ForMember(dest => dest.WorkArea, opt => opt.MapFrom(src => src.WorkArea.CatalogName))
             .ForMember(dest => dest.WorkEmail, opt => opt.MapFrom(src => src.WorkEmail))
             .ForMember(dest => dest.WorkPhoneNumber, opt => opt.MapFrom(src => src.WorkPhoneNumber))
             .ForMember(dest => dest.EntryDate, opt => opt.MapFrom(src => src.EntryDate))
             .ForMember(dest => dest.WorkPosition, opt => opt.MapFrom(src => src.WorkPosition.CatalogName))
             .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.BranchInfo.BranchName));

         CreateMap<Salary, SalaryInformationDto>()
             .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.AmountSalary))
             .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency.ToString()))
             .ForMember(dest => dest.SalaryType, opt => opt.MapFrom(src => src.SalaryType.ToString()));

         CreateMap<Vacation, VacationInformationDto>()
             .ForMember(dest => dest.AvailableVacations, opt => opt.MapFrom(src => src.AvailableVacations));

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
             .ForMember(dest => dest.VacationInformation, opt => opt.MapFrom(src => src.Vacation))
             .ForMember(dest => dest.SalaryInformation, opt => opt.MapFrom(src => src.Salaries != null ? src.Salaries.FirstOrDefault() : null))

             .AfterMap((src, dest) =>
             {
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
            SecondName = StringExtensions.FormatWithNullWhenNoHasValue(command.SecondName),
            ThirdName = StringExtensions.FormatWithNullWhenNoHasValue(command.ThirdName),
            FirstLastname = command.FirstLastname,
            SecondLastname = StringExtensions.FormatWithNullWhenNoHasValue(command.SecondLastname),
            IdentificationNumber = command.IdentificationNumber,
            IdentificationType = command.IdentificationType,
            Gender = command.Gender,
            Status = command.Status,
            CollaboratorCode = generatedCode,
            RegisteredBy = command.RegisteredBy ?? "Sistema ERP",
            DoesWorkSaturdays = command.DoesWorkSaturday,
            IsFirstTimeRegister = true,
            PictureUrl = null
         };
      }

      public static PersonalInformation ToPersonalInformationEntity(this Commands.PersonalInformation info, Guid collaboratorId)
      {
         return new PersonalInformation
         {
            Id = Guid.NewGuid(),
            CollaboratorId = collaboratorId,
            Address = StringExtensions.FormatWithNullWhenNoHasValue(info.Address),
            PersonalEmail = StringExtensions.FormatWithNullWhenNoHasValue(info.PersonalEmail),
            PersonalPhoneNumber = StringExtensions.FormatWithNullWhenNoHasValue(info.PersonalPhoneNumber),
            DepartamentId = info.DepartamentId,
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
            CompanyBranchId = info.BranchId,
            BankAccountNumber = StringExtensions.FormatWithNullWhenNoHasValue(info.BankAccountNumber),
            WorkPhoneNumber = StringExtensions.FormatWithNullWhenNoHasValue(info.WorkPhoneNumber),
            WorkEmail = StringExtensions.FormatWithNullWhenNoHasValue(info.WorkEmail),
            InssNumber = StringExtensions.FormatWithNullWhenNoHasValue(info.InssNumber),
            EntryDate = info.EntryDate,
            Daem = info.Daem
         };
      }

      #endregion Mapeo para crear colaborador
   }
}