using AutoMapper;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Dtos;

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

            CreateMap<PermitApplication, PermitApplicationDto>()
                .ForMember(dest => dest.ApprovedBy, opt => opt.MapFrom(src => src.ApprovedBy))
                .ForMember(dest => dest.RejectedBy, opt => opt.MapFrom(src => src.RejectedBy))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.RequestedBy, opt => opt.MapFrom(src => src.RequestedBy))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.PermitApllicationId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));
        }
    }
}   