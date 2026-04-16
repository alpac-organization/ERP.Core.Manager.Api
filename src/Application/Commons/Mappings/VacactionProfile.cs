using AutoMapper;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos;
using ERP.Core.Database.Domain.Entities.Payrolls;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class VacationProfile : Profile
    {
        public VacationProfile()
        {
            CreateMap<(Vacation, Collaborator), VacationDto>()
               .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => 
                    string.Join(" ", new[] 
                    { 
                        src.Item2.FirstName != null ? src.Item2.FirstName.ToCapitalize() : null, 
                        src.Item2.SecondName != null ? src.Item2.SecondName.ToCapitalize() : null, 
                        src.Item2.FirstLastname != null ? src.Item2.FirstLastname.ToCapitalize() : null, 
                        src.Item2.SecondLastname != null ? src.Item2.SecondLastname.ToCapitalize() : null 
                    }.Where(s => !string.IsNullOrWhiteSpace(s)))))
                .ForMember(dest => dest.AvailableVacations, opt => opt.MapFrom(src => src.Item1.AvailableVacations))
                .ForMember(dest => dest.GeneredVacation, opt => opt.MapFrom(src => src.Item1.GeneredVacation))
                .ForMember(dest => dest.EnjoyedVacation, opt => opt.MapFrom(src => src.Item1.EnjoyedVacation));

            CreateMap<PermitApplication, VacationControlDto>()
                .ForMember(dest => dest.CollaboratorCode, opt => opt.MapFrom(src => src.Collaborator.CollaboratorCode))
                .ForMember(dest => dest.WorkPosition, opt => opt.MapFrom(src => src.Collaborator.WorkingInformation.WorkPosition.CatalogName))
                .ForMember(dest => dest.PermitApplicationType, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.AmountDays, opt => opt.MapFrom(src => src.AmountDays))
                .ForMember(dest => dest.PermitApplicationId, opt => opt.MapFrom(src => src.Id))


                .ForMember(dest => dest.IdentificationCollaboratorToReceive, opt => opt.MapFrom(src => src.IdentificationCollaboratorToReceive))
                .ForMember(dest => dest.ApprovedBy, opt => opt.MapFrom(src => src.AdministratorFullName))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))

                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))

                .ForMember(dest => dest.CollaboratorFullname, opt => opt.MapFrom(src => 
                    string.Join(" ", new[] { 
                        src.Collaborator.FirstName, src.Collaborator.SecondName, src.Collaborator.FirstLastname, src.Collaborator.SecondLastname 
                    }
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.ToCapitalize())))
                );

        }
    }
}   