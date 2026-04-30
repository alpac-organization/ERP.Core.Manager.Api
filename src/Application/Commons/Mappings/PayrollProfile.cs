using AutoMapper;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class PayrollProfile : Profile
    {
        public PayrollProfile()
        {
            CreateMap<Payroll, PayrollPeriodDto>()
                .ForMember(dest => dest.PayrollId, src => src.MapFrom(or => or.Id))
                .ForMember(dest => dest.StartDate, src => src.MapFrom(or => or.StartDate))
                .ForMember(dest => dest.BranchName, src => src.MapFrom(or => or.Branch.BranchName))
                .ForMember(dest => dest.EndDate, src => src.MapFrom(or => or.EndDate))
                .ForMember(dest => dest.Type, src => src.MapFrom(or => or.PayrollType));
        }
    }
}