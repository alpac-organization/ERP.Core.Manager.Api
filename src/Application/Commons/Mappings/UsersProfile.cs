using AutoMapper;
using ERP.Core.Database.Domain.Entities.Auth;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Dtos;

using Commands = ERP.Core.Manager.Api.Application.Features.Users.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class UsersProfile : Profile
    {
        public UsersProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Fullname))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.UserStatus.ToString()));
        }
    }

    public static class UserMapper
    {
        public static User ToUserEntity(this Commands.CreateNewUserCommand command)
        {
            return new()
            {
                Id                   = Guid.NewGuid(),
                UserStatus           = UserStatus.Active,
                AreaId               = command.AreaId,
                Email                = command.Email,
                BranchId             = command.BranchId,
                IdentificationNumber = command.IdentificationNumber,
                UserType             = command.UserType,
                Fullname             = command.FullName,
            };
        }
    }
}