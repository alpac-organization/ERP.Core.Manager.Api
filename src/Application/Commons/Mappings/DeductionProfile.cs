using AutoMapper;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class DeductionProfile : Profile
    {
        public DeductionProfile()
        {
            CreateMap<Deduction, DeductionDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CollaboratorId, opt => opt.MapFrom(src => src.CollaboratorId));
        }
    }
}