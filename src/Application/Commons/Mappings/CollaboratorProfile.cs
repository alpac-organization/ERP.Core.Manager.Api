using AutoMapper;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class CollaboratorProfile : Profile
    {
        public CollaboratorProfile()
        {
            CreateMap<RegisterCollaboratorCommand, Collaborator>()
                .ForMember(dest => dest.CollaboratorCode, opt => opt.MapFrom(src => src.Code))
                
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.FirstLastname, opt => opt.MapFrom(src => src.FirstLastname))
                .ForMember(dest => dest.IdentificationNumber, opt => opt.MapFrom(src => src.IdentificationNumber))
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
                
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.RegisteredBy, opt => opt.MapFrom(src => src.RegisteredBy ?? "Sistema ERP"));

        }
    }
}