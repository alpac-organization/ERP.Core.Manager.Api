using AutoMapper;
using ERP.Core.Database.Domain.Entities.Payroll;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class PermitApplicationProfile : Profile
    {
        public PermitApplicationProfile()
        {
            CreateMap<PermitApplication, PermitApplicationDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.PermitApllicationId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CollaboratorCode, opt => opt.MapFrom(src => src.CollaboratorCode))
                .ForMember(dest => dest.FirtsStepApproved, opt => opt.MapFrom(src => src.FirtsStepApproved))
                .ForMember(dest => dest.SecondStepApproved, opt => opt.MapFrom(src => src.SecondStepApproved))
                .ForMember(dest => dest.IdentificationCollaboratorToReceive, opt => opt.MapFrom(src => src.IdentificationCollaboratorToReceive))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.RequestedBy, opt => opt.MapFrom(src => src.RequestedBy))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
                
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.PermitApllicationId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));
        }
    }
}   