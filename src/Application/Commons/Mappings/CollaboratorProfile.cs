using AutoMapper;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Utils;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;

using Commands = ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;
using ERP.Core.Manager.Api.Application.Commons.Utils;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{

   public class CollaboratorProfile : Profile
   {
      public CollaboratorProfile()
      {

        #region Mapeo de listado de colaboradores

         CreateMap<Collaborator, CollaboratorDto>()
            .ForMember(dest => dest.CollaboratorId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => ManagerUtils.FromSliceToCollaboratorFullname(src)))
            .ForMember(dest => dest.Vacations, opt => opt.MapFrom(src => src.Vacation.AvailableVacations))
            .ForMember(dest => dest.JobPosition, opt => opt.MapFrom(src => src.WorkingInformation.JobPosition.JobPositionName))
            .ForMember(dest => dest.CostCenter, opt => opt.MapFrom(src => src.WorkingInformation.CostCenter.CostCenterName))
            .ForMember(dest => dest.WorkArea, opt => opt.MapFrom(src => src.WorkingInformation.Area.WorkAreaName));

        #endregion

        #region Mapeo de detalles de colaborador

        CreateMap<PersonalInformation, PersonalInformationDto>()
            .ForMember(dest => dest.PersonalInformationId, opt => opt.MapFrom(src => src.Id));

        CreateMap<WorkingInformation, WorkingInformationDto>()
            .ForMember(dest => dest.WorkingInformationId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.WorkArea, opt => opt.MapFrom(src => src.Area.WorkAreaName))
            .ForMember(dest => dest.CostCenter, opt => opt.MapFrom(src => src.CostCenter.CostCenterName))
            .ForMember(dest => dest.JobPosition, opt => opt.MapFrom(src => src.JobPosition.JobPositionName))
         ;

        CreateMap<Salary, SalaryInformationDto>()
            .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.AmountSalary))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency.ToString()))
            .ForMember(dest => dest.SalaryType, opt => opt.MapFrom(src => src.SalaryType.ToString()));

        CreateMap<Vacation, VacationInformationDto>()
            .ForMember(dest => dest.AvailableVacations, opt => opt.MapFrom(src => src.AvailableVacations));

        CreateMap<Collaborator, CollaboratorDetailsDto>()
            .ForMember(dest => dest.CollaboratorId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CollaboratorCode, opt => opt.MapFrom(src => src.CollaboratorCode))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => ManagerUtils.FromSliceToCollaboratorFullname(src)))
            .ForMember(dest => dest.WorkPosition, opt => opt.MapFrom(src => src.WorkingInformation.JobPosition.JobPositionName))

            .ForMember(dest => dest.PersonalInformation, opt => opt.MapFrom(src => src.PersonalInformation))
            .ForMember(dest => dest.WorkingInformation, opt => opt.MapFrom(src => src.WorkingInformation))
            .ForMember(dest => dest.VacationInformation, opt => opt.MapFrom(src => src.Vacation))
            .ForMember(dest => dest.SalaryInformation, opt => opt.MapFrom(src => src.Salaries != null ? src.Salaries.FirstOrDefault() : null));

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
            Status = CollaboratorStatus.Active,
            CollaboratorCode = generatedCode,
            DoesWorkSaturdays = command.DoesWorkSaturday,
            IsFirstTimeRegister = true,
            PictureUrl = null
         };
      }

      public static PersonalInformation ToPersonalInformationEntity(this Commands.PersonalInformationCommand info, Guid collaboratorId)
      {
         return new PersonalInformation
         {
            Id = Guid.NewGuid(),
            CollaboratorId = collaboratorId,
            Address = StringExtensions.FormatWithNullWhenNoHasValue(info.Address),
            PersonalEmail = StringExtensions.FormatWithNullWhenNoHasValue(info.PersonalEmail),
            PersonalPhoneNumber = StringExtensions.FormatWithNullWhenNoHasValue(info.PersonalPhoneNumber),
            Gender = info.Gender,
            Birthdate = info.Birthdate,
            MaritalStatus = info.MaritalStatus
         };
      }

      public static WorkingInformation ToWorkingInformationEntity(this Commands.WorkingInformationCommand info, Guid collaboratorId)
      {
         return new WorkingInformation
         {
            Id = Guid.NewGuid(),
            CollaboratorId = collaboratorId,
            Daem = info.Daem,
            AreaId = info.AreaId,
            BranchId = info.BranchId,
            CostCenterId = info.CostCenterId,
            JobPositionId = info.JobPositionId,
            BankAccountNumber = StringExtensions.FormatWithNullWhenNoHasValue(info.BankAccountNumber),
            WorkPhoneNumber = StringExtensions.FormatWithNullWhenNoHasValue(info.WorkPhoneNumber),
            WorkEmail = StringExtensions.FormatWithNullWhenNoHasValue(info.WorkEmail),
            InssNumber = StringExtensions.FormatWithNullWhenNoHasValue(info.InssNumber),
            EntryDate = info.EntryDate
         };
      }

      #endregion Mapeo para crear colaborador
   }
}