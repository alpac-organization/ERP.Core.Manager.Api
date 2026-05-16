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
                .ForMember(dest => dest.VacationId, opt => opt.MapFrom(src => src.Item1.Id))
                .ForMember(dest => dest.GeneredVacation, opt => opt.MapFrom(src => src.Item1.GeneredVacation))
                .ForMember(dest => dest.EnjoyedVacation, opt => opt.MapFrom(src => src.Item1.EnjoyedVacation));

            CreateMap<Vacation, VacationAccruals>()
                .ForMember(dest => dest.VacationId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.VacationBalance, opt => opt.MapFrom(src => src.AvailableVacations))
                .ForMember(dest => dest.EnjoyedVacations, opt => opt.MapFrom(src => src.EnjoyedVacation))
                .ForMember(dest => dest.CollaboratorInformation, opt => opt.MapFrom(src => new CollaboratorInformation
                {
                    CollaboratorId = src.CollaboratorId,
                    IdentificationNumber = src.Collaborator.IdentificationNumber,
                    Code = src.Collaborator.CollaboratorCode,
                    WorkAreaName = src.Collaborator.WorkingInformation.WorkArea.CatalogName,
                    EntryDate = src.Collaborator.WorkingInformation != null 
                                ? src.Collaborator.WorkingInformation.EntryDate 
                                : DateTime.MinValue,
                    CollaboratorFullname = FormatFullName(
                        src.Collaborator.FirstName, 
                        src.Collaborator.SecondName, 
                        src.Collaborator.FirstLastname, 
                        src.Collaborator.SecondLastname)
            }));
        }

        private static string FormatFullName(params string?[] names)
        {
            return string.Join(" ", names
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!.Trim().ToCapitalize()));
        }

    }
}   