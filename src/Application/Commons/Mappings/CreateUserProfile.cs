using AutoMapper;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Auth;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class CreateUserProfile : Profile
    {
        public CreateUserProfile()
        {
            CreateMap<CreateNewUserCommand, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.Fullname, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.UserStatus, opt => opt.MapFrom(_ => UserStatus.Active))
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.WorkArea, opt => opt.Ignore())
                .ForMember(dest => dest.Branch, opt => opt.Ignore())
                .ForMember(dest => dest.Sessions, opt => opt.Ignore())
                .ForMember(dest => dest.Suppliers, opt => opt.Ignore())
                .ForMember(dest => dest.Quotations, opt => opt.Ignore())
                .ForMember(dest => dest.Profiles, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseRequests, opt => opt.Ignore());
        }
    }
}