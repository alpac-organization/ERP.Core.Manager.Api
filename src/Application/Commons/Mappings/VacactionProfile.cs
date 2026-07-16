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
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => ManagerUtils.FromSliceToCollaboratorFullname(src.Item2)))
                .ForMember(dest => dest.AvailableVacations, opt => opt.MapFrom(src => src.Item1.AvailableVacations))
                .ForMember(dest => dest.VacationId, opt => opt.MapFrom(src => src.Item1.Id))
                .ForMember(dest => dest.GeneredVacation, opt => opt.MapFrom(src => src.Item1.GeneredVacation))
                .ForMember(dest => dest.EnjoyedVacation, opt => opt.MapFrom(src => src.Item1.EnjoyedVacation));
        }
    }
}   