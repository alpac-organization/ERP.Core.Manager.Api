using AutoMapper;
using ERP.Core.Database.Domain.Entities.Auth;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class LoginProfile : Profile
    {
        public LoginProfile()
        {
            CreateMap<User, LoginDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Fullname))
                .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.UserType.ToString()))
                .ForMember(dest => dest.AccessToken, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyInformation, opt => opt.Ignore());
        }
    }
}