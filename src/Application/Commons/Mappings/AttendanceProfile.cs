using AutoMapper;
using ERP.Core.Clock.Database.Domain.Entities.Attendance;
using ERP.Core.Manager.Api.Application.Features.Attendance.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class AttendanceProfile : Profile
    {
        public AttendanceProfile()
        {
            CreateMap<Reading, AttendanceDto>()
                .ForMember(dest => dest.CollaboratorFullname, opt => opt.MapFrom(src => src.Employee.Name))
                .ForMember(dest => dest.ReadTime, opt => opt.MapFrom(src => src.ReadTime));
        }
    }
}