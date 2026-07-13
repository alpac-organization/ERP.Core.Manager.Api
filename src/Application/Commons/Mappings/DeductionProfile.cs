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
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.DeductionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CollaboratoFullname, opt => opt.MapFrom(src => ManagerUtils.FromSliceToCollaboratorFullname(src.Collaborator)))
                .ForMember(dest => dest.IdentificationNumber, opt => opt.MapFrom(src => src.Collaborator.IdentificationNumber));


            CreateMap<Deduction, DeductionDetailsDto>()
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
                .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AmountPaid))
                .ForMember(dest => dest.AmountPaidInDollars, opt => opt.MapFrom(src => src.AmountPaidInDollars))
                .ForMember(dest => dest.FortnightlyAmount, opt => opt.MapFrom(src => src.FortnightlyAmount))
                .ForMember(dest => dest.FortnightlyAmountInDollars, opt => opt.MapFrom(src => src.FortnightlyAmountInDollars))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.NumberFortnights, opt => opt.MapFrom(src => src.NumberFortnights))
                .ForMember(dest => dest.NumberFortnightsPaid, opt => opt.MapFrom(src => src.NumberFortnightsPaid))
                .ForMember(dest => dest.TotalBalance, opt => opt.MapFrom(src => src.TotalBalance))
                .ForMember(dest => dest.TotalBalanceInDollars, opt => opt.MapFrom(src => src.TotalBalanceInDollars))
                .ForMember(dest => dest.DeductionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.TotalAmountInDollars, opt => opt.MapFrom(src => src.TotalAmountInDollars));


            CreateMap<DeductionPaymentHistory, DeductionPaymentsDto>()
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
                .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AmountPaid))
                .ForMember(dest => dest.AmountPaidInDollars, opt => opt.MapFrom(src => src.AmountPaidInDollars))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => src.Origin))
                .ForPath(dest => dest.DeductionDetails.StartDate, opt => opt.MapFrom(src => src.Payroll.StartDate))
                .ForPath(dest => dest.DeductionDetails.EndDate, opt => opt.MapFrom(src => src.Payroll.EndDate))
                .ForPath(dest => dest.DeductionDetails.PayrollId, opt => opt.MapFrom(src => src.Payroll.Id));
        }
    }
}