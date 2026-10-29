using AutoMapper;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class DeductionProfile : Profile
    {
        public DeductionProfile()
        {
            CreateMap<Deduction, DeductionDto>()
                .ForMember(dest => dest.DeductionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CollaboratoFullname, opt => opt.MapFrom(src => ManagerUtils.FromSliceToCollaboratorFullname(src.Collaborator)))
                .ForMember(dest => dest.IdentificationNumber, opt => opt.MapFrom(src => src.Collaborator.IdentificationNumber));

            CreateMap<Deduction, DeductionDetailsDto>()
                .ForMember(dest => dest.DeductionId, opt => opt.MapFrom(src => src.Id));
                
            CreateMap<DeductionPaymentHistory, DeductionPaymentsDto>()
                .ForPath(dest => dest.DeductionDetails.StartDate, opt => opt.MapFrom(src => src.Payroll.StartDate))
                .ForPath(dest => dest.DeductionDetails.EndDate, opt => opt.MapFrom(src => src.Payroll.EndDate))
                .ForPath(dest => dest.DeductionDetails.PayrollId, opt => opt.MapFrom(src => src.Payroll.Id));
        }
    }
}