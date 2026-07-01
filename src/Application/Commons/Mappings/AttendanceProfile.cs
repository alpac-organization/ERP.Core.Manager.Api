using AutoMapper;
using ERP.Core.Clock.Database.Domain.Entities.Attendance;
using ERP.Core.Manager.Api.Application.Features.Attendance.v1.Dtos;
using static ERP.Core.Manager.Api.Application.Features.Attendance.v1.Dtos.AttendanceDto;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class AttendanceProfile : Profile
    {
        public AttendanceProfile()
        {
            CreateMap<Reading, AttendanceDto>()
                .ForMember(dest => dest.CollaboratorFullname, opt => opt.MapFrom(src => src.Employee.Name));

             CreateMap<Reading, MarkingDto>()
                .ForMember(dest => dest.ReadTime, opt => opt.MapFrom(src => src.ReadTime))
                .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.Device.DeviceName));
        }
    }
}