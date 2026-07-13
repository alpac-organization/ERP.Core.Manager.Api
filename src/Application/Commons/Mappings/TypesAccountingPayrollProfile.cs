using AutoMapper;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class TypesAccountingPayrollProfile : Profile
    {
        public TypesAccountingPayrollProfile()
        {
            CreateMap<TypesAccountingPayroll, TypesAccountingPayrollDto>()
                .ForMember(dest => dest.AccountingPayrollId, src => src.MapFrom(or => or.Id));
        }
    }
}